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

[QueryProperty(nameof(Order), "Order")]
public partial class OrderStatusViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderTitle))]
    private OrderDto _order;

    public string OrderTitle => Order is null
        ? string.Empty
        : $"#{Order.OrderIdentifierNumber} rendelés";

    // WebSocket logic goes here later
}