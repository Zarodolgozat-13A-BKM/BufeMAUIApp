using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BufeApp.Controls;
using CommunityToolkit.Maui.Views;

namespace BufeApp.Services
{
    public static class ApiService
    {
        public static string BaseUrl = "https://bufeapi.jcloud.jedlik.cloud/api/";
        //endpoints
        public static string LoginEndpoint = "account/login";
        public static string LogoutEndpoint = "account/logout";
        public static string CategoriesEndpoint = "categories";
        public static string MeEndpoint = "account/me";

        private static LoadingPopup _loadingPopup;
        private static int _loadingCounter = 0;

        private static void ShowLoading()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _loadingCounter++;
                if (_loadingPopup != null) return;
                _loadingPopup = new LoadingPopup();
                Application.Current?.MainPage?.ShowPopup(_loadingPopup);
            });
        }

        private static void HideLoading()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _loadingCounter--;
                if (_loadingCounter <= 0)
                {
                    _loadingCounter = 0;
                    if (_loadingPopup != null)
                    {
                        _loadingPopup.Close();
                        _loadingPopup = null;
                    }
                }
            });
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public static async Task<T> GetAsync<T>(string endpoint, string bearerToken = null)
        {
            try
            {
                ShowLoading();
                var client = new HttpClient();
                client.BaseAddress = new Uri(BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(bearerToken))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
                }
                var response = await client.GetAsync(endpoint);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) { UserService.UserUnauthorised(); }
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(jsonResponse, GetJsonOptions());
            }
            finally
            {
                HideLoading();
            }
        }

        public static async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, string bearerToken = null)
        {
            try
            {
                ShowLoading();
                var client = new HttpClient();
                client.BaseAddress = new Uri(BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(bearerToken))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
                }
                var jsonData = JsonSerializer.Serialize(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) { UserService.UserUnauthorised(); }
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(jsonResponse, GetJsonOptions());
            }
            finally
            {
                HideLoading();
            }
        }
    }
}
