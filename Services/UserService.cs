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

        public static async Task LoginUser(string Email, string Password)
        {
            var loginRequest = new { email = Email, password = Password };
            var loginResponse = await ApiService.PostAsync<object, LoginResponse>(ApiService.LoginEndpoint, loginRequest);
            if(loginResponse != null && !string.IsNullOrEmpty(loginResponse.AccessToken))
            {
                BearerToken = loginResponse.AccessToken;
                //SetUserData(); // Implement this method to fetch and set user data
                await StorageService.SetSecureValue("BearerToken", BearerToken);
            }
            else
            {
                throw new Exception("Login failed");
            }
        }

        public static async Task SetUserData()
        {
            throw new NotImplementedException();
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

        public static async Task RegisterUser(string Name, string Email, string Password, string PasswordConfirmation)
        {
            var registerRequest = new { name = Name, email = Email, password = Password, password_confirmation = PasswordConfirmation };
            var registerResponse = await ApiService.PostAsync<object, RegisterResponse>(ApiService.RegisterEndpoint, registerRequest);
            if (registerResponse != null && !string.IsNullOrEmpty(registerResponse.AccessToken))
            {
                BearerToken = registerResponse.AccessToken;
                //SetUserData(); // Implement this method to fetch and set user data
                await StorageService.SetSecureValue("BearerToken", BearerToken);
            }
            else
            {
                throw new Exception("Registration failed");
            }
        }
    }
}
