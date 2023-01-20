using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Ecomail
{
    /// <summary>
    /// Represents plugin settings
    /// </summary>
    public class EcomailSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the API key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use tracking
        /// </summary>
        public bool UseTracking { get; set; }

        /// <summary>
        /// Gets or sets the tracking script
        /// </summary>
        public string TrackingScript { get; set; }

        /// <summary>
        /// Gets or sets the App Id
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of list to synchronize contacts
        /// </summary>
        public int ListId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of GDPR newsletter consent
        /// </summary>
        public int ConsentId { get; set; }

        #region Advanced

        /// <summary>
        /// Gets or sets a period (in seconds) before the request times out
        /// </summary>
        public int? RequestTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log tracking errors
        /// </summary>
        public bool LogTrackingErrors { get; set; }

        /// <summary>
        /// Gets or sets a page size of contacts to synchronize
        /// </summary>
        public int SyncPageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to export contacts from account to the store
        /// </summary>
        public bool ExportContactsOnSync { get; set; }

        /// <summary>
        /// Gets or sets a period (in hours) to rebuild product feed
        /// </summary>
        public int RebuildFeedXmlAfterHours { get; set; }

        #endregion
    }
}