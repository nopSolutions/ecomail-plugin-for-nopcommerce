using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domain.Api
{
    public class TransactionCreateRequest
    {
        [JsonProperty("transaction")]
        public TransactionData TransactionData { get; set; }

        [JsonProperty("transaction_items")]
        public List<TransactionItem> TransactionItems { get; set; }
    }

    public class TransactionData
    {
        [JsonIgnore]
        public int OrderId { get; set; }

        [JsonProperty("order_id")]
        public string OrderNumber { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("shop")]
        public string Shop { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("tax")]
        public string Tax { get; set; }

        [JsonProperty("shipping")]
        public string Shipping { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("county")]
        public string County { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }
    }
}
