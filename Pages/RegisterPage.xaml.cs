namespace BufeApp.Pages;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await MyIconControl.Animate();
    }

    private void Button_Clicked2(object sender, EventArgs e)
    {
        // Reset progress to 0 (start) and play
        CheckAnimation.Progress = TimeSpan.Zero;
        CheckAnimation.IsAnimationEnabled = true;
    }
}