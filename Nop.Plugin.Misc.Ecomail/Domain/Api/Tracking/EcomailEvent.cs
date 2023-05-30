using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domain.Api.Tracking
{
    public class EcomailEvent
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
