using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BufeApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BufeApp.Services
{
    public static class CartService
    {
        public static ObservableCollection<CartItemModel> Items { get; set; } = new();

        public static decimal TotalPrice => Items.Sum(i => i.TotalPrice);

        public static event EventHandler CartChanged;

        public static void AddItem(ItemModel item, int quantity)
        {
            var existingItem = Items.FirstOrDefault(i => i.Item.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItemModel(item, quantity);
                cartItem.PropertyChanged += (s, e) => {
                    if (e.PropertyName == nameof(CartItemModel.TotalPrice) || e.PropertyName == nameof(CartItemModel.Quantity))
                    {
                        CartChanged?.Invoke(null, EventArgs.Empty);
                    }
                };
                Items.Add(cartItem);
            }
            CartChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void RemoveItem(CartItemModel item)
        {
            Items.Remove(item);
            CartChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void ClearCart()
        {
            Items.Clear();
            CartChanged?.Invoke(null, EventArgs.Empty);
        }

        public static OrderRequestModel CreateOrderRequest(string comment, string deliveryDateText, bool isCash)
        {
            
            var request = new OrderRequestModel
            {
                DeliveryDate = deliveryDateText,
                Comment = comment,
                Items = Items.Select(i => new OrderItemRequestModel
                {
                    ItemId = i.Item.Id,
                    Quantity = i.Quantity
                }).ToList(),
                Cash = isCash
            };
            return request;
        }

        public static async void ReorderFromOrder(OrderModel order)
        {
            ClearCart();
            foreach (var item in order.Items)
            {
                int orderedItemId = item.ItemId;
                bool IsActive = true;
                ItemResponseModel tmp = await ApiService.GetAsync<ItemResponseModel>($"{ApiService.ItemEndpoint}/{orderedItemId}", UserService.BearerToken);
                if (tmp.Item.IsActiveInt == 0) IsActive = false;
                if (!IsActive)
                {
                    await Application.Current.MainPage.DisplayAlert("Áru nem elérhető", "Sajnos az alábbi termék jelenleg nem elérhető: " + item.ItemName, "OK");
                }
                else
                {
                    var itemModel = new ItemModel
                    {
                        Id = item.ItemId,
                        Name = item.ItemName,
                        Price = item.ItemPrice,
                        PictureUrl = item.PictureUrl
                    };
                    AddItem(itemModel, item.Quantity);
                }
            }
        }
    }
}
