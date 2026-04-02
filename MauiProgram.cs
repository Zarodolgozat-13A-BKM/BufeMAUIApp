using BufeApp.Pages;
using BufeApp.Services;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.BottomSheet.Hosting;

namespace BufeApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseBottomSheet()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<CartPage>();
            builder.Services.AddTransient<OrderStatusViewModel>();
            builder.Services.AddTransient<OrderStatusPage>();

            // ViewModels
            builder.Services.AddTransient<MainViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
        // Removes the underline on Android
        handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
                // Removes the border/box style on iOS
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif WINDOWS
        // Removes the default border on Windows
        handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
#endif
            });

            return builder.Build();
        }
    }
}
