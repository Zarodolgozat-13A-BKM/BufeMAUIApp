namespace BufeApp.Pages;

public partial class PasswordResetConfirmPage : ContentPage
{
	public PasswordResetConfirmPage()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (await Launcher.CanOpenAsync(new Uri("mailto:")))
            {
                await Launcher.OpenAsync(new Uri("mailto:"));
            }
            else
            {
                await DisplayAlert("Hiba", "Nincs telepített e-mail alkalmazás.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hiba", "Nem sikerült megnyitni az e-mail alkalmazást.", "OK");
        }
    }
}