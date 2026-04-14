using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BufeApp.Models;
using BufeApp.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace BufeApp.Pages;

public partial class OrderStatusPage : ContentPage
{
    private readonly OrderStatusViewModel _vm;

    public OrderStatusPage(OrderStatusViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.StartTrackingAsync();
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _vm.StopTrackingAsync();
    }
}

public class OrderItemDisplayModel
{
    public string QuantityLabel { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string PriceLabel { get; init; } = string.Empty;
}

// Holds the visual state of a single stepper step
public partial class StepState : ObservableObject
{
    [ObservableProperty] private Color background = Colors.Transparent;
    [ObservableProperty] private Color iconColor = Colors.Gray;
    [ObservableProperty] private Color labelColor = Colors.Gray;
}

[QueryProperty("OrderIdentifierNumber", "OrderId")]
public partial class OrderStatusViewModel : ObservableObject
{
    private const string StatusWaitingForPayment = "fizetésre vár";
    private const string StatusPaid = "fizetve";
    private const string StatusBeingPrepared = "készítjük";
    private const string StatusReadyForPickup = "átvehető";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderTitle))]
    private int orderIdentifierNumber;

    public string OrderTitle => orderIdentifierNumber == 0
        ? string.Empty
        : $"#{orderIdentifierNumber} rendelés";

    [ObservableProperty] private string deliveryDateLabel = "–";
    [ObservableProperty] private string totalLabel = "–";

    public ObservableCollection<OrderItemDisplayModel> OrderItems { get; } = new();

    // Steps[0] = "fizetésre vár", [1] = "fizetve", [2] = "készítjük", [3] = "átvehető"
    public ObservableCollection<StepState> Steps { get; } = new(
        Enumerable.Range(0, 4).Select(_ => new StepState())
    );

    [ObservableProperty] private Color wsStatusColor = Colors.Gray;
    [ObservableProperty] private string wsStatusText = "Csatlakozás…";

    private PusherService? _pusher;

    public async Task StartTrackingAsync()
    {
        ShowCachedOrderData();

        // Channel name matches the web app pattern: private-ordersOfUser.<base64(email)>
        var emailBase64 = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(UserService.Email ?? string.Empty));
        var channelName = $"private-ordersOfUser.{emailBase64}";

        _pusher = new PusherService();
        _pusher.ConnectionStateChanged += OnConnectionStateChanged;

        await _pusher.SubscribeAsync(
            channelName,
            eventName: "order.state.changed",
            onEventReceived: OnOrderStateChangedAsync);
    }

    public async Task StopTrackingAsync()
    {
        if (_pusher is not null)
        {
            _pusher.ConnectionStateChanged -= OnConnectionStateChanged;
            await _pusher.DisconnectAsync();
            _pusher = null;
        }
    }

    private void ShowCachedOrderData()
    {
        var cachedOrder = UserService.Orders
            .FirstOrDefault(o => o.OrderIdentifierNumber == OrderIdentifierNumber);

        if (cachedOrder is null) return;

        UpdateOrderDetails(cachedOrder);
        UpdateStepper(cachedOrder.Status);
    }

    private async Task OnOrderStateChangedAsync(string eventDataJson)
    {
        using var doc = JsonDocument.Parse(eventDataJson);

        if (!doc.RootElement.TryGetProperty("order_id", out var orderIdEl))
            return;

        var orderId = orderIdEl.GetInt32();

        // Fetch the latest version of all our orders from the API
        var freshOrders = await ApiService.GetAsync<List<OrderModel>>(
            ApiService.OrdersEndpoint, UserService.BearerToken);

        // Find the specific order we are currently displaying
        var updatedOrder = freshOrders?.FirstOrDefault(
            o => o.OrderIdentifierNumber == OrderIdentifierNumber);

        if (updatedOrder is null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateOrderDetails(updatedOrder);
            UpdateStepper(updatedOrder.Status);
            WsStatusText = $"Frissítve: {updatedOrder.Status}";
        });
    }

    private void OnConnectionStateChanged(bool isConnected)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            WsStatusColor = isConnected ? GetAppResource<Color>("Primary") : Colors.Orange;
            WsStatusText = isConnected ? "Élő frissítés" : "Újracsatlakozás…";
        });
    }

    private void UpdateOrderDetails(OrderModel order)
    {
        DeliveryDateLabel = order.FormattedDate ?? "–";
        TotalLabel = $"{order.TotalPrice:N0} Ft";

        OrderItems.Clear();
        foreach (var item in order.Items ?? [])
        {
            OrderItems.Add(new OrderItemDisplayModel
            {
                QuantityLabel = $"{item.Quantity}x",
                Name = item.ItemName ?? string.Empty,
                PriceLabel = $"{item.Price:N0} Ft",
            });
        }
    }

    private void UpdateStepper(string? status)
    {
        var primaryColor = GetAppResource<Color>("Primary");
        var onPrimaryColor = GetAppResource<Color>("OnPrimary");
        var inactiveColor = GetAppResource<Color>("SurfaceVariant");
        var inactiveFgColor = GetAppResource<Color>("OnSurfaceVariant");

        var currentStep = status?.ToLowerInvariant() switch
        {
            StatusWaitingForPayment => 1,
            StatusPaid => 2,
            StatusBeingPrepared => 3,
            StatusReadyForPickup => 4,
            _ => 1,
        };

        for (int i = 0; i < Steps.Count; i++)
        {
            bool isActive = i < currentStep;
            Steps[i].Background = isActive ? primaryColor : inactiveColor;
            Steps[i].IconColor = isActive ? onPrimaryColor : inactiveFgColor;
            Steps[i].LabelColor = isActive ? primaryColor : inactiveFgColor;
        }
    }

    private static T GetAppResource<T>(string key)
    {
        if (Application.Current!.Resources.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return default!;
    }

    [RelayCommand]
    private async Task GoHome()
    {
        await StopTrackingAsync();
        await Shell.Current.GoToAsync("//MainPage");
    }
}