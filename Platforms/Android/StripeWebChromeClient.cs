using Android.Webkit;
using Microsoft.Maui.Controls;

namespace BufeApp.Platforms.Android
{
    public class StripeWebChromeClient : WebChromeClient
    {
        public override bool OnConsoleMessage(ConsoleMessage consoleMessage)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[WebView] {consoleMessage.SourceId()}:{consoleMessage.LineNumber()} " +
                $"{consoleMessage.Message()}");
            return true;
        }
    }
}