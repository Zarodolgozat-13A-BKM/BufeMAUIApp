using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BufeApp.Models
{
    public class PaginatedResponse<T>
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }

        [JsonPropertyName("meta")]
        public PaginationMeta Meta { get; set; }

        [JsonPropertyName("links")]
        public PaginationLinks Links { get; set; }
    }

    public class PaginationMeta
    {
        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("last_page")]
        public int LastPage { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("per_page")]
        public int PerPage { get; set; }

        [JsonPropertyName("from")]
        public int From { get; set; }

        [JsonPropertyName("to")]
        public int To { get; set; }
    }

    public class PaginationLinks
    {
        [JsonPropertyName("first")]
        public string First { get; set; }

        [JsonPropertyName("last")]
        public string Last { get; set; }

        [JsonPropertyName("prev")]
        public string Prev { get; set; }

        [JsonPropertyName("next")]
        public string Next { get; set; }
    }
}