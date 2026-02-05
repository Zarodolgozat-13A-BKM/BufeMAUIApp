using BufeApp.Services;

namespace BufeApp.Pages;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
		((Button)sender).IsEnabled = false;
        try 
		{
            await UserService.LoginUser(Email_Entry.Text, Password_Entry.Text);
			await DisplayAlert("Login Successful", $"You have been logged in.", "OK");
			await Shell.Current.GoToAsync("//MainPage");
            ((Button)sender).IsEnabled = true;
            return;
        } 
		catch 
		{
			await DisplayAlert("Login Failed", "Invalid email or password.", "OK");
            ((Button)sender).IsEnabled = true;
            return;
        }
        
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        // Navigate to the PasswordResetPage
		await Shell.Current.GoToAsync(nameof(PasswordResetPage));
    }
}