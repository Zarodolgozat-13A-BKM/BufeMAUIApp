using BufeApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufeApp.Services
{
    public static class UserService
    {
        public static string BearerToken { get; set; }
        public static string Email { get; set; }
        public static string Name { get; set; }
        public static string ReverbKey { get; set; }

        public static List<OrderModel> Orders { get; set; } = new();
        public static int CurrentPage { get; set; } = 1;
        public static int LastPage { get; set; } = 1;
        public static int TotalOrders { get; set; } = 0;

        public static bool IsUserLoggedIn()
        {
            return !string.IsNullOrEmpty(BearerToken);
        }

        public static async Task GetTokenFromStorage()
        {
            BearerToken = await StorageService.GetSecureValue("BearerToken");
        }

        public static async Task LoginUser(string Username, string Password)
        {
            var loginRequest = new { username = Username, password = Password };
            var loginResponse = await ApiService.PostAsync<object, LoginResponse>(ApiService.LoginEndpoint, loginRequest);
            if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.AccessToken))
            {
                BearerToken = loginResponse.AccessToken;
                await SetUserData();
                await StorageService.SetSecureValue("BearerToken", BearerToken);
            }
            else
            {
                throw new Exception("Login failed");
            }
        }

        public static async Task SetUserData()
        {
            var userData = await ApiService.GetAsync<UserDataResponseModel>(ApiService.MeEndpoint, BearerToken);
            ReverbKeyModel reverbKey = await ApiService.GetAsync<ReverbKeyModel>(ApiService.ReverbKeyEndpoint, BearerToken);
            if (userData != null)
            {
                Name = userData.full_name;
                Email = userData.email;
                ReverbKey = reverbKey.key;
                await LoadOrdersAsync();
            }
            else
            {
                await UserUnauthorised();
            }
        }

        public static async Task UserUnauthorised()
        {
            await StorageService.SetSecureValue("BearerToken", string.Empty);
            BearerToken = null;
            Email = null;
            Name = null;
            await Application.Current.MainPage.DisplayAlert("Hiba", "Hiba történt, kérlek jelentkezz be újra!", "Ok");
            await Shell.Current.GoToAsync("//LoginPage");
        }

        public static async Task LogoutUser()
        {
            var logoutResponse = await ApiService.PostAsync<object, object>(ApiService.LogoutEndpoint, null, BearerToken);
            if (logoutResponse != null)
            {
                BearerToken = null;
                Email = null;
                Name = null;
                await StorageService.SetSecureValue("BearerToken", string.Empty);
            }
            else
            {
                throw new Exception("Logout failed");
            }
        }

        public static async Task LoadOrdersAsync(int page = 1)
        {
            var result = await ApiService.GetAsync<PaginatedResponse<OrderModel>>(
                $"{ApiService.OrdersEndpoint}?page={page}", BearerToken);

            if (result != null)
            {
                Orders = result.Data ?? new List<OrderModel>();
                CurrentPage = result.Meta?.CurrentPage ?? page;
                LastPage = result.Meta?.LastPage ?? 1;
                TotalOrders = result.Meta?.Total ?? Orders.Count;
            }
            else
            {
                Orders = new List<OrderModel>();
                CurrentPage = 1;
                LastPage = 1;
                TotalOrders = 0;
            }
        }
    }
}