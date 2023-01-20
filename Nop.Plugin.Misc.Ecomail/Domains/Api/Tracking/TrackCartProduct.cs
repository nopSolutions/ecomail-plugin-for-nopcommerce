using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domains.Api.Tracking
{
    public class TrackCartProduct
    {
        [JsonProperty("productId")]
        public int ProductId { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("img_url")]
        public string ImgUrl { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("fullDescription")]
        public string FullDescription { get; set; }
    }
}
