using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domains.Api
{
    public class ContactListResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string ListName { get; set; }

        [JsonProperty("from_name")]
        public string FromName { get; set; }

        [JsonProperty("from_email")]
        public string FromEmail { get; set; }

        [JsonProperty("reply_to")]
        public string ReplyTo { get; set; }

        [JsonProperty("errors")]
        public string Errors { get; set; }
    }
}
