using CommunityToolkit.Mvvm.ComponentModel;
using BufeApp.Models;

namespace BufeApp.Models
{
    public partial class CartItemModel : ObservableObject
    {
        [ObservableProperty]
        private ItemModel _item;

        [ObservableProperty]
        private int _quantity;

        public decimal TotalPrice => Item?.Price * Quantity ?? 0;

        public CartItemModel(ItemModel item, int quantity)
        {
            _item = item;
            _quantity = quantity;
        }

        partial void OnQuantityChanged(int value)
        {
            OnPropertyChanged(nameof(TotalPrice));
        }
    }
}