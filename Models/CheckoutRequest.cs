using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BufeApp.Models
{
    public class CheckoutRequest
    {
        [JsonPropertyName("delivery_date")]
        public string DeliveryDate { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("cash")]
        public bool Cash { get; set; }

        [JsonPropertyName("items")]
        public List<CheckoutItem> Items { get; set; }
    }

    public class CheckoutItem
    {
        [JsonPropertyName("item_id")]
        public int ItemId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
