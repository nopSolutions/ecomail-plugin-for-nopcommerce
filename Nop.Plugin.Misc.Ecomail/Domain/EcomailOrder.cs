using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Ecomail.Domain
{
    /// <summary>
    /// Represents a synchronized order 
    /// </summary>
    public class EcomailOrder : BaseEntity
    {
        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets transaction identifier (unknown when the order is synced in bulk)
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the API key of account for which the order is synced
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the sync date
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}