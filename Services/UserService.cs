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
            if(loginResponse != null && !string.IsNullOrEmpty(loginResponse.AccessToken))
            {
                BearerToken = loginResponse.AccessToken;
                await SetUserData(); // Implement this method to fetch and set user data
                await StorageService.SetSecureValue("BearerToken", BearerToken);
            }
            else
            {
                throw new Exception("Login failed");
            }
        }

        public static async Task SetUserData()
        {
            await ApiService.GetAsync<UserDataResponseModel>(ApiService.MeEndpoint, BearerToken).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var userData = task.Result;
                    Name = userData.full_name;
                    Email = userData.email;
                }
                else
                {
                    throw new Exception("Failed to fetch user data");
                }
            });
        }

        public static async Task UserUnauthorised()
        {
            await StorageService.SetSecureValue("BearerToken", string.Empty);
            BearerToken = null;
            Email = null;
            Name = null;
            await Shell.Current.GoToAsync("//LoginPage");
        }

        public static async Task LogoutUser()
        {
            //send post to logout endpoint with bearer token
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

        //public static async Task RegisterUser(string Name, string Email, string Password, string PasswordConfirmation)
        //{
        //    var registerRequest = new { name = Name, email = Email, password = Password, password_confirmation = PasswordConfirmation };
        //    var registerResponse = await ApiService.PostAsync<object, RegisterResponse>(ApiService.RegisterEndpoint, registerRequest);
        //    if (registerResponse != null && !string.IsNullOrEmpty(registerResponse.AccessToken))
        //    {
        //        BearerToken = registerResponse.AccessToken;
        //        //SetUserData(); // Implement this method to fetch and set user data
        //        await StorageService.SetSecureValue("BearerToken", BearerToken);
        //    }
        //    else
        //    {
        //        throw new Exception("Registration failed");
        //    }
        //}
    }
}
