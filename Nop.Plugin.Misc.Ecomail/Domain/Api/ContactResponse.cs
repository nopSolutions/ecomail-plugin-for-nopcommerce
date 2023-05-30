using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domain.Api
{
    public class ContactResponse
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string FirstName { get; set; }

        [JsonProperty("surname")]
        public string SecondName { get; set; }

        [JsonProperty("lists")]
        public Dictionary<string, ListInfo> Lists { get; set; }
    }

    public class ListInfo
    {
        [JsonProperty("list_id")]
        public int ListId { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("subscribed_at")]
        public DateTimeOffset? SubscribedAt { get; set; }

        [JsonProperty("unsubscribed_at")]
        public DateTimeOffset? UnsubscribedAt { get; set; }

        [JsonProperty("last_activity_at")]
        public DateTimeOffset? LastActivityAt { get; set; }
    }
}