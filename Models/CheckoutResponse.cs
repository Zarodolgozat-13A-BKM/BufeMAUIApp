using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BufeApp.Models
{
    public class CheckoutResponse
    {
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; }

        [JsonPropertyName("order")]
        public OrderDto Order { get; set; }
    }

    public class OrderDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("order_identifier_number")]
        public int OrderIdentifierNumber { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("delivery_date")]
        public string DeliveryDate { get; set; }

        [JsonPropertyName("total_price")]
        public string TotalPrice { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        [JsonPropertyName("item_id")]
        public int ItemId { get; set; }

        [JsonPropertyName("item_name")]
        public string ItemName { get; set; }

        [JsonPropertyName("quantity")]
        public string Quantity { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }
    }

    public class StripeKeyResponse
    {
        [JsonPropertyName("publishable_key")]
        public string PublishableKey { get; set; }
    }
}
