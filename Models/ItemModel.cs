using System;
using System.Text.Json.Serialization;

namespace BufeApp.Models
{
    public class ItemModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("picture_url")]
        public string? PictureUrl { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("is_active")]
        public int IsActiveInt { get; set; }
        
        [JsonIgnore]
        public bool IsActive => IsActiveInt == 1;

        [JsonPropertyName("default_time_to_deliver")]
        public int DefaultTimeToDeliver { get; set; }

        [JsonPropertyName("is_featured")]
        public int IsFeaturedInt { get; set; }
        
        [JsonIgnore]
        public bool IsFeatured => IsFeaturedInt == 1;

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
