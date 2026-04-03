using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BufeApp.Models
{
    public class OrderModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("order_identifier_number")]
        public int OrderIdentifierNumber { get; set; }

        [JsonPropertyName("user_username")]
        public string UserUsername { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("delivery_date")]
        public string DeliveryDate { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItemModel> Items { get; set; }

        [JsonPropertyName("total_price")]
        public int TotalPrice { get; set; }

        [JsonPropertyName("default_completion_time")]
        public int DefaultCompletionTime { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("payment_intent_id")]
        public string PaymentIntentId { get; set; }

        public string StatusLabel => Status?.ToUpper() switch
        {
            "PENDING" => "FÜGGŐBEN",
            "PAID" => "FIZETVE",
            "PREPARING" => "KÉSZÜL",
            "READY" => "ÁTVEHETŐ",
            "COMPLETED" => "SIKERES",
            "CANCELLED" => "LEMONDVA",
            _ => Status?.ToUpper() ?? "—"
        };

        public string ItemsSummary => Items == null || Items.Count == 0
            ? "Nincs tétel"
            : string.Join(" + ", Items.Select(i => $"{i.Quantity} x {i.ItemName}"));

        public string FormattedDate => DateTime.TryParse(DeliveryDate, out var dt)
            ? dt.ToString("yyyy. MM. dd.")
            : DeliveryDate;

        public bool IsCompleted => Status is "completed" or "cancelled";
        public bool IsInProgress => !IsCompleted;
    }

    public class OrderItemModel
    {
        [JsonPropertyName("item_id")]
        public int ItemId { get; set; }

        [JsonPropertyName("item_name")]
        public string ItemName { get; set; }

        [JsonPropertyName("item_price")]
        public int ItemPrice { get; set; }

        [JsonPropertyName("picture_url")]
        public string PictureUrl { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }
    }
}
