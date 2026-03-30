using BufeApp.Models;
using BufeApp.Services;
using System.Collections.ObjectModel;

namespace BufeApp.Pages;

public partial class CartPage : ContentPage
{
	private bool _isTimePickerExpanded;
	private bool _breaksLoaded;

	private bool _isBuffetOpen = true;
	public bool IsBuffetOpen
	{
		get => _isBuffetOpen;
		set
		{
			if (_isBuffetOpen != value)
			{
				_isBuffetOpen = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsBuffetClosed));
			}
		}
	}
	public bool IsBuffetClosed => !IsBuffetOpen;

	public decimal TotalPrice => CartService.TotalPrice;

	public bool IsCartEmpty => !CartService.Items.Any();
	public bool IsCartNotEmpty => CartService.Items.Any();

	private List<Break> _allBreaks = new();

    public ObservableCollection<Break> Breaks { get; set; } = new();

	public CartPage()
	{
		InitializeComponent();
		BindingContext = this;

		CartService.CartChanged += (s, e) =>
		{
			OnPropertyChanged(nameof(TotalPrice));
			OnPropertyChanged(nameof(IsCartEmpty));
			OnPropertyChanged(nameof(IsCartNotEmpty));
		};
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (!_breaksLoaded)
		{
			var response = await ApiService.GetAsync<BreakResponseModel>(ApiService.BreaksEndpoint, UserService.BearerToken);
			if (response?.breaks != null)
			{
				_allBreaks = response.breaks.ToList();
            }
		}
        FilterValidBreaks();
    }

	private void FilterValidBreaks()
	{
        bool isDev = true;
        var now = DateTime.Now.TimeOfDay;
		Breaks.Clear();

		foreach (var b in _allBreaks)
		{
			if (TimeSpan.TryParse(b.start, out var startTime))
			{
				if (startTime > now || isDev)
				{
					Breaks.Add(b);
				}
			}
			else
			{
				Breaks.Add(b);
			}
		}
		_breaksLoaded = true;

		

		IsBuffetOpen = Breaks.Count > 0;
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

	private async void OnTimeOptionSelected(object sender, EventArgs e)
	{
		if (sender is Label label)
		{
			SelectedTimeLabel.Text = label.Text;

            await Task.WhenAll(
                TimeOptionsContainer.FadeTo(0, 200),
                ChevronIcon.RotateTo(0, 200)
            );
            TimeOptionsContainer.IsVisible = false;
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
        if (sender is View view && view.BindingContext is Models.CartItemModel cartItem)
        {
            Services.CartService.RemoveItem(cartItem);
        }
    }

    
}