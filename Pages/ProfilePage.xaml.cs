using BufeApp.Services;
using BufeApp.Models;
using Microsoft.Maui.Storage;

namespace BufeApp.Pages;

public partial class ProfilePage : ContentPage
{
    public string Username => UserService.Name;
    public string Email => UserService.Email;

    public List<OrderModel> Orders => UserService.Orders
        .OrderByDescending(x => x.OrderIdentifierNumber)
        .ToList();

    public bool HasNoOrders => UserService.Orders.Count == 0;

    // Pagination
    public int CurrentPage => UserService.CurrentPage;
    public int LastPage => UserService.LastPage;
    public bool HasPrevPage => UserService.CurrentPage > 1;
    public bool HasNextPage => UserService.CurrentPage < UserService.LastPage;
    public bool HasMultiplePages => UserService.LastPage > 1;
    public string PageInfo => $"{UserService.CurrentPage} / {UserService.LastPage}  ({UserService.TotalOrders} db)";

    public Command<OrderModel> ReorderCommand => new Command<OrderModel>(async (order) =>
    {
        CartService.ReorderFromOrder(order);
        await Shell.Current.GoToAsync("//CartPage");
    });

    public Command<OrderModel> ViewStatusCommand => new Command<OrderModel>(async (order) =>
    {
        await Shell.Current.GoToAsync($"{nameof(OrderStatusPage)}?OrderId={order.OrderIdentifierNumber}");
    });

    public Command NextPageCommand => new Command(async () =>
    {
        if (!HasNextPage) return;
        await LoadPage(UserService.CurrentPage + 1);
    });

    public Command PrevPageCommand => new Command(async () =>
    {
        if (!HasPrevPage) return;
        await LoadPage(UserService.CurrentPage - 1);
    });

    public ProfilePage()
    {
        InitializeComponent();

        this.BindingContext = this;

        var savedTheme = Preferences.Default.Get("AppTheme", (int)Application.Current.UserAppTheme);
        AppTheme currentTheme = (AppTheme)savedTheme;

        Application.Current.UserAppTheme = currentTheme;
        UpdateThemeUI(currentTheme);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        OnPropertyChanged(nameof(Username));
        OnPropertyChanged(nameof(Email));

        await LoadPage(1);
    }

    private async Task LoadPage(int page)
    {
        await UserService.LoadOrdersAsync(page);
        OnPropertyChanged(nameof(Orders));
        OnPropertyChanged(nameof(HasNoOrders));
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(LastPage));
        OnPropertyChanged(nameof(HasPrevPage));
        OnPropertyChanged(nameof(HasNextPage));
        OnPropertyChanged(nameof(HasMultiplePages));
        OnPropertyChanged(nameof(PageInfo));
    }

    private void OnThemeTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string themeString && Enum.TryParse(themeString, out AppTheme theme))
        {
            Application.Current.UserAppTheme = theme;
            Preferences.Default.Set("AppTheme", (int)theme);
            UpdateThemeUI(theme);
        }
    }

    private void UpdateThemeUI(AppTheme selectedTheme)
    {
        // Reset all to default
        LightModeBorder.BackgroundColor = Colors.Transparent;
        DarkModeBorder.BackgroundColor = Colors.Transparent;
        SystemModeBorder.BackgroundColor = Colors.Transparent;

        LightModeIcon.SetAppThemeColor(Label.TextColorProperty, (Color)Application.Current.Resources["TextSecLight"], (Color)Application.Current.Resources["TextSecDark"]);
        LightModeLabel.SetAppThemeColor(Label.TextColorProperty, (Color)Application.Current.Resources["TextSecLight"], (Color)Application.Current.Resources["TextSecDark"]);

        DarkModeIcon.SetAppThemeColor(Label.TextColorProperty, (Color)Application.Current.Resources["TextSecLight"], (Color)Application.Current.Resources["TextSecDark"]);
        DarkModeLabel.SetAppThemeColor(Label.TextColorProperty, (Color)Application.Current.Resources["TextSecLight"], (Color)Application.Current.Resources["TextSecDark"]);

        SystemModeIcon.SetAppThemeColor(Label.TextColorProperty, (Color)Application.Current.Resources["TextSecLight"], (Color)Application.Current.Resources["TextSecDark"]);
        SystemModeLabel.SetAppThemeColor(Label.TextColorProperty, (Color)Application.Current.Resources["TextSecLight"], (Color)Application.Current.Resources["TextSecDark"]);

        // Highlight selected
        var primaryColor = (Color)Application.Current.Resources["Primary"];
        var whiteColor = (Color)Application.Current.Resources["White"];

        switch (selectedTheme)
        {
            case AppTheme.Light:
                LightModeBorder.BackgroundColor = primaryColor;
                LightModeIcon.TextColor = whiteColor;
                LightModeLabel.TextColor = whiteColor;
                break;
            case AppTheme.Dark:
                DarkModeBorder.BackgroundColor = primaryColor;
                DarkModeIcon.TextColor = whiteColor;
                DarkModeLabel.TextColor = whiteColor;
                break;
            case AppTheme.Unspecified:
            default:
                SystemModeBorder.BackgroundColor = primaryColor;
                SystemModeIcon.TextColor = whiteColor;
                SystemModeLabel.TextColor = whiteColor;
                break;
        }
    }

    private async void Logout_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            await UserService.LogoutUser();
            await Application.Current.MainPage.DisplayAlert("Success", "Logged out successfully", "OK");
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}