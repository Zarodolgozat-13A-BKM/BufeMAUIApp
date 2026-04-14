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

    [ObservableProperty] private Color step1Background = Colors.Transparent;
    [ObservableProperty] private Color step1IconColor = Colors.Gray;
    [ObservableProperty] private Color step1LabelColor = Colors.Gray;

    [ObservableProperty] private Color step2Background = Colors.Transparent;
    [ObservableProperty] private Color step2IconColor = Colors.Gray;
    [ObservableProperty] private Color step2LabelColor = Colors.Gray;

    [ObservableProperty] private Color step3Background = Colors.Transparent;
    [ObservableProperty] private Color step3IconColor = Colors.Gray;
    [ObservableProperty] private Color step3LabelColor = Colors.Gray;

    [ObservableProperty] private Color step4Background = Colors.Transparent;
    [ObservableProperty] private Color step4IconColor = Colors.Gray;
    [ObservableProperty] private Color step4LabelColor = Colors.Gray;

    [ObservableProperty] private Color wsStatusColor = Colors.Gray;
    [ObservableProperty] private string wsStatusText = "Csatlakozás…";

    private PusherService? _pusher;

    public async Task StartTrackingAsync()
    {
        ShowCachedOrderData();

        // The channel name follows the same pattern as the web app:
        // private-ordersOfUser.<base64(email)>
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

        // Update the UI on the main thread
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

        // Map the status string to a step number (1–4)
        var currentStep = status?.ToLowerInvariant() switch
        {
            StatusWaitingForPayment => 1,
            StatusPaid => 2,
            StatusBeingPrepared => 3,
            StatusReadyForPickup => 4,
            _ => 1,
        };

        // Apply colours to each step
        for (int step = 1; step <= 4; step++)
        {
            bool isActive = step <= currentStep;
            var bg = isActive ? primaryColor : inactiveColor;
            var fg = isActive ? onPrimaryColor : inactiveFgColor;
            var label = isActive ? primaryColor : inactiveFgColor;

            switch (step)
            {
                case 1: (Step1Background, Step1IconColor, Step1LabelColor) = (bg, fg, label); break;
                case 2: (Step2Background, Step2IconColor, Step2LabelColor) = (bg, fg, label); break;
                case 3: (Step3Background, Step3IconColor, Step3LabelColor) = (bg, fg, label); break;
                case 4: (Step4Background, Step4IconColor, Step4LabelColor) = (bg, fg, label); break;
            }
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