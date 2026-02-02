using BufeApp.Services;

namespace BufeApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            InitializeNavigation();
        }

        private async void InitializeNavigation()
        {
            await UserService.GetTokenFromStorage();
            //await UserService.LogoutUser(); // For testing purposes only, remove in production

            if (UserService.IsUserLoggedIn())
            {
                // User is logged in, navigate to MainPage
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
