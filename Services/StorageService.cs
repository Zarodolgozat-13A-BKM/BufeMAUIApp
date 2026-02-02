using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufeApp.Services
{
    public static class StorageService
    {
        public static async Task SetSecureValue(string ValueName, string Value)
        {
            await SecureStorage.Default.SetAsync(ValueName, Value);
        }

        public static async Task<string> GetSecureValue(string ValueName)
        {
            return await SecureStorage.Default.GetAsync(ValueName); 
        }

        public static string GetValue(string ValueName)
        {
            return Preferences.Default.Get(ValueName, string.Empty);
        }
    }
}
