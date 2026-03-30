using BufeApp.Models;

namespace BufeApp.Pages;

public partial class PaymentWebViewPage : ContentPage
{
	private readonly CheckoutResponse _checkout;
    private readonly string _publishableKey;

    public PaymentWebViewPage(CheckoutResponse checkout, string publishableKey)
    {
        InitializeComponent();
        _checkout = checkout;
        _publishableKey = publishableKey;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        using var stream = await FileSystem.OpenAppPackageFileAsync("stripe_checkout.html");
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();

        var injection = $@"<script>
                                window.__STRIPE_PARAMS__ = {{
                                    clientSecret:   '{EscapeJs(_checkout.ClientSecret)}',
                                    publishableKey: '{EscapeJs(_publishableKey)}',
                                    amount:         '{EscapeJs(_checkout.Order.TotalPrice)}'
                                }};
                            </script>";

        html = html.Replace("</head>", injection + "\n</head>");
        StripeWebView.Source = new HtmlWebViewSource { Html = html };
    }

    private void OnNavigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("bufeapp://payment-success"))
        {
            e.Cancel = true;
            HandleSuccess();
        }
    }

    private void OnNavigated(object sender, WebNavigatedEventArgs e)
    {
        Loader.IsVisible = false;
        Loader.IsRunning = false;
    }

    private async void HandleSuccess()
    {
        await Navigation.PopModalAsync();
        await Shell.Current.GoToAsync($"../{nameof(OrderStatusPage)}?OrderId={_checkout.Order.Id}");
    }

    protected override bool OnBackButtonPressed() => true;

    private static string EscapeJs(string s) =>
        s?.Replace("\\", "\\\\").Replace("'", "\\'") ?? string.Empty;
}