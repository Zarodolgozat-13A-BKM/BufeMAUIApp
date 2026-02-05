using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BufeApp.Pages;

public partial class PasswordResetPage : ContentPage
{

    public PasswordResetPage()
	{
		InitializeComponent();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        Email_Entry.Text = string.Empty;
        Shell.Current.GoToAsync(nameof(PasswordResetConfirmPage));
    }

   
}