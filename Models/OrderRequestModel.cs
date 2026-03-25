using System.Text.Json.Serialization;

namespace BufeApp.Models
{
    public class OrderRequestModel
    {
        [JsonPropertyName("delivery_date")]
        public string DeliveryDate { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItemRequestModel> Items { get; set; } = new();
    }

    public class OrderItemRequestModel
    {
        [JsonPropertyName("item_id")]
        public int ItemId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}