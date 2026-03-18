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
        ErrorBox.IsVisible = false;
        try 
		{
            await UserService.LoginUser(Name_Entry.Text, Password_Entry.Text);
			await Shell.Current.GoToAsync("//MainPage");
            ((Button)sender).IsEnabled = true;
            return;
        } 
		catch (Exception ex)
		{
            ErrorLabel.Text = "Hibás bejelentkezési adatok vagy szerverhiba";
            ErrorBox.IsVisible = true;
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