using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domains.Api
{
    public class ContactListSubscribersInfoResponse
    {
        [JsonProperty("unknown")]
        public long Unknown { get; set; }

        [JsonProperty("subscribed")]
        public long Subscribed { get; set; }

        [JsonProperty("unsubscribed")]
        public long Unsubscribed { get; set; }

        [JsonProperty("soft_bounced")]
        public long SoftBounced { get; set; }

        [JsonProperty("hard_bounced")]
        public long HardBounced { get; set; }

        [JsonProperty("complained")]
        public long Complained { get; set; }

        [JsonProperty("unconfirmed")]
        public long Unconfirmed { get; set; }
    }
}
