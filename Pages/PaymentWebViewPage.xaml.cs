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

#if ANDROID
    var androidWebView = (StripeWebView.Handler?.PlatformView as Android.Webkit.WebView);
    if (androidWebView != null)
    {
        androidWebView.Settings.JavaScriptEnabled = true;
        androidWebView.Settings.DomStorageEnabled = true;
        androidWebView.SetWebChromeClient(new Platforms.Android.StripeWebChromeClient());
    }
#endif

        using var stream = await FileSystem.OpenAppPackageFileAsync("stripe_checkout.html");
        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();

        var injection = $@"<script>
                                window.__STRIPE_PARAMS__ = {{
                                    clientSecret:   '{EscapeJs(_checkout.ClientSecret)}',
                                    publishableKey: '{EscapeJs(_publishableKey)}',
                                    amount:         '{EscapeJs(_checkout.Order.TotalPrice.ToString())}'
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

    private async void HandleSuccess()
    {
        await Navigation.PopModalAsync();
        await Task.Delay(300);

        await Shell.Current.GoToAsync($"//MainPage");
        await Shell.Current.GoToAsync($"{nameof(OrderStatusPage)}?OrderId={_checkout.Order.Id}");
    }

    protected override bool OnBackButtonPressed() => true;

    private static string EscapeJs(string s) =>
        s?.Replace("\\", "\\\\").Replace("'", "\\'") ?? string.Empty;
}