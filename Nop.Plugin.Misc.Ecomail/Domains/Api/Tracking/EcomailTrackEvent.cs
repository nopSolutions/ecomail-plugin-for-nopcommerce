using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domains.Api.Tracking
{
    public class EcomailTrackEvent
    {
        [JsonProperty("event")]
        public EcomailEvent Event { get; set; }
    }
}
