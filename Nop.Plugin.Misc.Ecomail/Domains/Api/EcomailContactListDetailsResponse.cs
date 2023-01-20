using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domains.Api
{
    public class EcomailContactListDetailsResponse
    {

        [JsonProperty("list")]
        public ContactListDetailInfo ContactListInfo { get; set; }

        [JsonProperty("subscribers")]
        public ContactListSubscribersInfoResponse Subscribers { get; set; }
    }

    public class ContactListDetailInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("brand_id")]
        public object BrandId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("from_name")]
        public string FromName { get; set; }

        [JsonProperty("from_email")]
        public string FromEmail { get; set; }

        [JsonProperty("reply_to")]
        public string ReplyTo { get; set; }

        [JsonProperty("active_subscribers")]
        public long ActiveSubscribers { get; set; }
    }

}
