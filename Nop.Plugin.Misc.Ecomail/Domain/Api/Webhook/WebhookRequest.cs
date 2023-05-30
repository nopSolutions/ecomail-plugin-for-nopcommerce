using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domain.Api.Webhook
{
    public class WebhookRequest
    {
        [JsonProperty("payload")]
        public WebhookRequestPayload WebhookPayload { get; set; }
    }

    public class WebhookRequestPayload
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("listId")]
        public int ListId { get; set; }

        [JsonProperty("campaignId")]
        public int? CampaignId { get; set; }
    }
}
