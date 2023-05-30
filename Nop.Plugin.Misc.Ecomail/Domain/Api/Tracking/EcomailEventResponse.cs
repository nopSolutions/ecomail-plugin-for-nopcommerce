using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domain.Api.Tracking
{
    public class EcomailEventResponse : EcomailEvent
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
