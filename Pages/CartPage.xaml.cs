namespace BufeApp.Pages;

public partial class CartPage : ContentPage
{
	private bool _isTimePickerExpanded;

    public decimal TotalPrice => Services.CartService.TotalPrice;

	public CartPage()
	{
		InitializeComponent();
        BindingContext = this;

        Services.CartService.CartChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(IsCartEmpty));
            OnPropertyChanged(nameof(IsCartNotEmpty));
        };
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

    private void OnIncreaseQuantity(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Models.CartItemModel cartItem)
        {
            if (cartItem.Quantity < 99)
            {
                cartItem.Quantity++;
            }
        }
    }

    private void OnDecreaseQuantity(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Models.CartItemModel cartItem)
        {
            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }
            else
            {
                Services.CartService.RemoveItem(cartItem);
            }
        }
    }

    private void OnDeleteItem(object sender, EventArgs e)
    {
        var view = sender as View;
        var gesture = view?.GestureRecognizers.OfType<TapGestureRecognizer>().FirstOrDefault();
        var parameter = gesture?.CommandParameter;

        if (parameter is Models.CartItemModel cartItem)
        {
             Services.CartService.RemoveItem(cartItem);
        }
        else if (sender is Button button && button.BindingContext is Models.CartItemModel btnCartItem)
        {
             Services.CartService.RemoveItem(btnCartItem);
        }
        else if(view?.BindingContext is Models.CartItemModel viewCartItem)
        {
             Services.CartService.RemoveItem(viewCartItem);
        }
    }

    public bool IsCartEmpty => !Services.CartService.Items.Any();
    public bool IsCartNotEmpty => Services.CartService.Items.Any();
}