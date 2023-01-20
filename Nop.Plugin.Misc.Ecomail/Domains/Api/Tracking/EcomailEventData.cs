using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domains.Api.Tracking
{
    public class EcomailEventData
    {
        [JsonProperty("data")]
        public object Data { get; set; }
    }
}
