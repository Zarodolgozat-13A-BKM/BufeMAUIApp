using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BufeApp.Services;

public class PusherService
{
    // ── Server connection settings ───────────────────────────
    private const string AppKey = "lkmdfbj4dsd2tyfuprvn";
    private const string WebSocketHost = "bufeapi-ws.jcloud.jedlik.cloud";
    private const string AuthEndpoint = "https://bufeapi.jcloud.jedlik.cloud/broadcasting/auth";

    // ── State ────────────────────────────────────────────────
    private ClientWebSocket? _socket;
    private CancellationTokenSource? _cancellation;

    // ── Public event ─────────────────────────────────────────

    /// Fires whenever the connection state changes (connected / reconnecting).
    public event Action<bool>? ConnectionStateChanged;

    // Subscribes to a private channel and starts listening for events.
    public async Task SubscribeAsync(
        string channelName,
        string eventName,
        Func<string, Task> onEventReceived)
    {
        _cancellation = new CancellationTokenSource();
        await ConnectAndListenAsync(channelName, eventName, onEventReceived, _cancellation.Token);
    }

    //  Cleanly closes the WebSocket connection.
    public async Task DisconnectAsync()
    {
        _cancellation?.Cancel();

        if (_socket is not null)
        {
            if (_socket.State == WebSocketState.Open)
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnected", CancellationToken.None);

            _socket.Dispose();
            _socket = null;
        }
    }

    //  Internal connection loop with auto-reconnect.
    private async Task ConnectAndListenAsync(
        string channelName,
        string eventName,
        Func<string, Task> onEventReceived,
        CancellationToken cancellationToken)
    {
        var connectionUri = new Uri($"wss://{WebSocketHost}/app/{AppKey}?protocol=7&client=maui&version=1.0");

        while (!cancellationToken.IsCancellationRequested)
        {
            _socket?.Dispose();
            _socket = new ClientWebSocket();

            try
            {
                // ── Step 1: Connect ──────────────────────────
                await _socket.ConnectAsync(connectionUri, cancellationToken);
                var socketId = await WaitForConnectionConfirmationAsync(cancellationToken);

                // ── Step 2: Authenticate the private channel ─
                var authToken = await AuthorizeChannelAsync(socketId, channelName, cancellationToken);

                // ── Step 3: Subscribe ────────────────────────
                await SubscribeToChannelAsync(channelName, authToken, cancellationToken);

                ConnectionStateChanged?.Invoke(true);

                // ── Receive loop ─────────────────────────────
                await ListenForEventsAsync(channelName, eventName, onEventReceived, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break; // DisconnectAsync was called — stop cleanly
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PusherService] Error: {ex.Message}");
                ConnectionStateChanged?.Invoke(false);

                // Wait 3 seconds before attempting to reconnect
                await Task.Delay(3000, cancellationToken).ContinueWith(_ => { });
            }
        }
    }

    //  Waits for the first message from the server
    private async Task<string> WaitForConnectionConfirmationAsync(CancellationToken cancellationToken)
    {
        var message = await ReadNextMessageAsync(cancellationToken);
        using var doc = JsonDocument.Parse(message);

        var eventName = doc.RootElement.GetProperty("event").GetString();
        if (eventName != "pusher:connection_established")
            throw new Exception($"Unexpected first message from server: {eventName}");

        // The "data" field is itself a JSON string — this is how the Pusher protocol works
        var dataJson = doc.RootElement.GetProperty("data").GetString()!;
        using var dataDoc = JsonDocument.Parse(dataJson);

        return dataDoc.RootElement.GetProperty("socket_id").GetString()!;
    }

    //  Calls the auth endpoint
    private async Task<string> AuthorizeChannelAsync(
        string socketId,
        string channelName,
        CancellationToken cancellationToken)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", UserService.BearerToken);

        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "socket_id",    socketId    },
            { "channel_name", channelName },
        });

        var response = await http.PostAsync(AuthEndpoint, body, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("auth").GetString()!;
    }

    //  Sends the "pusher:subscribe" message to join the channel.
    private async Task SubscribeToChannelAsync(
        string channelName,
        string authToken,
        CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Serialize(new
        {
            @event = "pusher:subscribe",
            data = new
            {
                channel = channelName,
                auth = authToken,
            }
        });

        await SendMessageAsync(message, cancellationToken);
    }

    //  Handles ping/pong keepaalives and forwards matching events to the caller.
    private async Task ListenForEventsAsync(
        string channelName,
        string targetEvent,
        Func<string, Task> onEventReceived,
        CancellationToken cancellationToken)
    {
        while (_socket!.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var message = await ReadNextMessageAsync(cancellationToken);
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;

            var eventName = root.GetProperty("event").GetString();

            switch (eventName)
            {
                // Server is checking we are still alive — reply immediately
                case "pusher:ping":
                    await SendMessageAsync("{\"event\":\"pusher:pong\",\"data\":{}}", cancellationToken);
                    break;

                // Forward any matching event to the caller
                case var name when name == targetEvent || name == $".{targetEvent}":
                    // Extract the data payload (Pusher double-encodes it as a JSON string)
                    var dataJson = root.GetProperty("data").GetString() ?? "{}";
                    await onEventReceived(dataJson);
                    break;
            }
        }
    }

    //  Reads a complete WebSocket message.
    private async Task<string> ReadNextMessageAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        var builder = new StringBuilder();
        WebSocketReceiveResult result;

        do
        {
            result = await _socket!.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
                throw new OperationCanceledException("Server closed the WebSocket connection.");

            builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
        }
        while (!result.EndOfMessage);

        return builder.ToString();
    }

    //  Sends a text message over the WebSocket.
    private async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        await _socket!.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken);
    }
}
