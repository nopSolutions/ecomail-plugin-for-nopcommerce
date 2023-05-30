using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domain.Api
{
    public class SubscriberDataRequest
    {
        public SubscriberDataRequest()
        {
            CustomFields = new Dictionary<string, string>();
        }

        [JsonProperty("name")]
        public string FirstName { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("vokativ")]
        public string Vokativ { get; set; }

        [JsonProperty("vokativ_s")]
        public string VokativS { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("pretitle")]
        public string Pretitle { get; set; }

        [JsonProperty("surtitle")]
        public string Surtitle { get; set; }

        [JsonProperty("birthday")]
        public string Birthday { get; set; }

        [JsonProperty("custom_fields")]
        public Dictionary<string, string> CustomFields { get; set; }
    }
}