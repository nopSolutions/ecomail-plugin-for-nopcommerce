using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domain.Api.Tracking
{
    public class EcomailEventData
    {
        [JsonProperty("data")]
        public object Data { get; set; }
    }
}
