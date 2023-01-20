using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domains.Api
{
    public class TransactionItem
    {
        [JsonProperty("code")]
        public string ItemCode { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("amount")]
        public int Quantity { get; set; }

        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }
    }
}
