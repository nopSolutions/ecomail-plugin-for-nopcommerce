using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domains.Api
{
    public class SubscriberAddRequest
    {
        [JsonProperty("subscriber_data")]
        public SubscriberDataRequest SubscriberData { get; set; }

        [JsonProperty("trigger_autoresponders")]
        public bool TriggerAutoresponders { get; set; }

        [JsonProperty("update_existing")]
        public bool UpdateExisting { get; set; }

        [JsonProperty("resubscribe")]
        public bool Resubscribe { get; set; }
    }
}
