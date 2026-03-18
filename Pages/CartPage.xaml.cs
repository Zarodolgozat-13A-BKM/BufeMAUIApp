namespace BufeApp.Pages;

public partial class CartPage : ContentPage
{
	private bool _isTimePickerExpanded;

	public CartPage()
	{
		InitializeComponent();
	}

	private async void OnTimePickerTapped(object sender, EventArgs e)
	{
		_isTimePickerExpanded = !_isTimePickerExpanded;

		if (_isTimePickerExpanded)
		{
			TimeOptionsContainer.IsVisible = true;
			await Task.WhenAll(
				TimeOptionsContainer.FadeTo(1, 200),
				ChevronIcon.RotateTo(180, 200)
			);
		}
		else
		{
			await Task.WhenAll(
				TimeOptionsContainer.FadeTo(0, 200),
				ChevronIcon.RotateTo(0, 200)
			);
			TimeOptionsContainer.IsVisible = false;
		}
	}

	private void OnTimeOptionSelected(object sender, EventArgs e)
	{
		if (sender is Label label)
		{
			SelectedTimeLabel.Text = label.Text;

			// Close the picker
			_isTimePickerExpanded = false;
			_ = Task.WhenAll(
				TimeOptionsContainer.FadeTo(0, 200),
				ChevronIcon.RotateTo(0, 200)
			).ContinueWith(t => 
			{
				MainThread.BeginInvokeOnMainThread(() => TimeOptionsContainer.IsVisible = false);
			});
		}
	}
}