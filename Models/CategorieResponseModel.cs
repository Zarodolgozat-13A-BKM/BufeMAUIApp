using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BufeApp.Models
{
    public class CategorieResponseModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("items")]
        public List<ItemModel> Items { get; set; } = new List<ItemModel>();
    }
}
