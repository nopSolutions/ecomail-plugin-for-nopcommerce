using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.Ecomail.Domain.Api
{
    public class TransactionsRequest
    {
        [JsonProperty("transaction_data")]
        public List<TransactionCreateRequest> Transactions { get; set; }
    }
}