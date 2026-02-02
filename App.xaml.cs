using BufeApp.Services;

namespace BufeApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Always use AppShell, which will handle the navigation logic
            MainPage = new AppShell();
        }
    }
}
