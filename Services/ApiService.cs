using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BufeApp.Services
{
    public static class ApiService
    {
        public static string BaseUrl = "https://bufeapi.jcloud.jedlik.cloud/api/";
        //endpoints
        public static string LoginEndpoint = "account/login";
        public static string LogoutEndpoint = "account/logout";
        public static string CategoriesEndpoint = "categories";

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public static async Task<T> GetAsync<T>(string endpoint, string bearerToken = null)
        {
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

        public static async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, string bearerToken = null)
        {
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
    }
}
