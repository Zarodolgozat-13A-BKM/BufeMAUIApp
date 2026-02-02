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
		try 
		{
            await UserService.LoginUser(Email_Entry.Text, Password_Entry.Text);
			DisplayAlert("Login Successful", $"You have been logged in. {UserService.BearerToken}", "OK");
			//await Shell.Current.GoToAsync("//MainPage");
        } 
		catch 
		{
			DisplayAlert("Login Failed", "Invalid email or password.", "OK");
			return;
        }
    }
}