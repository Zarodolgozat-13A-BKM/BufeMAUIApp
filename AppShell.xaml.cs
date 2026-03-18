using BufeApp.Services;

namespace BufeApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register routes for navigation
            Routing.RegisterRoute(nameof(Pages.PasswordResetPage), typeof(Pages.PasswordResetPage));

            InitializeNavigation();
        }

        private async void InitializeNavigation()
        {

            //await UserService.UserUnauthorised(); // For testing purposes only, remove in production
            // Check if user has access to internet, if not show alert and exit app
            var connectivity = Connectivity.Current;
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                bool shouldRetry = await DisplayAlert(
                    "Nincs internet kapcsolat",
                    "Internet kapcsolatra van szükség a használathoz!",
                    "Újra",
                    "Kilépés");

                if (shouldRetry)
                {
                    // Retry the initialization
                    InitializeNavigation();
                    return;
                }
                else
                {
                    // Exit the app
                    Application.Current?.Quit();
                    return;
                }
            }

            await UserService.GetTokenFromStorage();
            //await UserService.LogoutUser(); // For testing purposes only, remove in production

            if (UserService.IsUserLoggedIn())
            {
                // User is logged in, navigate to MainPage
                await UserService.SetUserData(); // Fetch and set user data before navigating
                await GoToAsync("//MainPage");
            }
            else
            {
                // User is not logged in, navigate to LoginPage
                await GoToAsync("//LoginPage");
            }
        }
    }
}
