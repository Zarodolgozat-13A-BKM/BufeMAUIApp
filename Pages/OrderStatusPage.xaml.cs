using CommunityToolkit.Mvvm.ComponentModel;
using BufeApp.Models;

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
}

[QueryProperty("OrderId", "OrderId")]
public partial class OrderStatusViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderTitle))]
    private int orderId;

    public string OrderTitle => OrderId == 0
        ? string.Empty
        : $"#{OrderId} rendelés";

    // WebSocket logic goes here later
}