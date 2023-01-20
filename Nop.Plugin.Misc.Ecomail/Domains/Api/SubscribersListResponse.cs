using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domains.Api
{
    public class SubscribersListResponse
    {
        public SubscribersListResponse()
        {
            SubscribersDataList = new List<SubscribersData>();
        }

        [JsonProperty("data")]
        public List<SubscribersData> SubscribersDataList { get; set; }

        [JsonProperty("from")]
        public int? From { get; set; }

        [JsonProperty("to")]
        public int? To { get; set; }

        [JsonProperty("current_page")]
        public int CurrentPage { get; set; }

        [JsonProperty("last_page")]
        public int LastPage { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class SubscribersData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("list_id")]
        public int ListId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("sms_status")]
        public int SmsStatus { get; set; }

        [JsonProperty("subscribed_at")]
        public DateTimeOffset SubscribedAt { get; set; }

        [JsonProperty("unsubscribed_at")]
        public object UnsubscribedAt { get; set; }
    }
}
