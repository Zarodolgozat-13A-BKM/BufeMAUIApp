using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BufeApp.Models
{
    public class ItemResponseModel
    {
        [JsonPropertyName("item")]
        public ItemModel Item { get; set; }
    }
}
