using BufeApp.Services;

namespace BufeApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Restore saved theme
            var savedTheme = Preferences.Default.Get("AppTheme", (int)AppTheme.Unspecified);
            Current.UserAppTheme = (AppTheme)savedTheme;

            // Always use AppShell, which will handle the navigation logic
            MainPage = new AppShell();
        }
    }
}
