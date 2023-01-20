using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domains.Api
{
    public class SubscribersAddOnBulkRequest
    {
        [JsonProperty("subscriber_data")]
        public List<SubscriberDataRequest> SubscriberDataList { get; set; }

        [JsonProperty("trigger_autoresponders")]
        public bool TriggerAutoresponders { get; set; }

        [JsonProperty("update_existing")]
        public bool UpdateExisting { get; set; }

        [JsonProperty("resubscribe")]
        public bool Resubscribe { get; set; }
    }
}
