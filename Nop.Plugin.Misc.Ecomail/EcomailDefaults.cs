using Nop.Core;

namespace Nop.Plugin.Misc.Ecomail
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public static class EcomailDefaults
    {
        /// <summary>
        /// Gets a name of the view component to embed tracking script on pages
        /// </summary>
        public const string TRACKING_VIEW_COMPONENT_NAME = "Widget_Ecomail_Tracking";

        /// <summary>
        /// Gets a plugin system name
        /// </summary>
        public static string SystemName => "Misc.Ecomail";

        /// <summary>
        /// Gets a user agent used to request third-party services
        /// </summary>
        public static string UserAgent => $"nopCommerce-{NopVersion.CURRENT_VERSION}";

        /// <summary>
        /// Gets a header of the API key authorization: key
        /// </summary>
        public static string ApiKeyHeader => "key";

        /// <summary>
        /// Gets a period (in seconds) before the request times out
        /// </summary>
        public static int RequestTimeout => 10;

        /// <summary>
        /// Gets a name of the webhook route
        /// </summary>
        public static string WebhookRoute => "Plugin.Misc.Ecomail.Webhook";

        /// <summary>
        /// Gets a name of the product feed route
        /// </summary>
        public static string ProductFeedRoute => "Plugin.Misc.Ecomail.ProductFeed";

        /// <summary>
        /// Gets the synchronization schedule task
        /// </summary>
        public static (string Name, string Type, int Period) SynchronizationTask =>
            ("Synchronization (Ecomail plugin)", "Nop.Plugin.Misc.Ecomail.Services.SynchronizationTask", 24);

        /// <summary>
        /// Gets a name of attribute to store a store identifier
        /// </summary>
        public static string SubscriberStoreIdAttribute => "SHOP_ID";

        /// <summary>
        /// Gets a name of attribute to store a store name
        /// </summary>
        public static string SubscriberStoreNameAttribute => "SHOP_NAME";

        /// <summary>
        /// Gets a name of attribute to store a store url
        /// </summary>
        public static string SubscriberStoreUrlAttribute => "SHOP_URL";

        /// <summary>
        /// Gets a key of the attribute to store shopping cart identifier
        /// </summary>
        public static string ShoppingCartGuidAttribute => $"{SystemName}.ShoppingCart.Guid";

        /// <summary>
        /// Gets a key of the attribute to store shopping cart
        /// </summary>
        public static string BasketActionAttribute => "Basket";

        /// <summary>
        /// Gets a name of the cart updated event
        /// </summary>
        public static string CartUpdatedEventName => "ECOMAIL_CART_UPDATED";

        /// <summary>
        /// Gets a name of the cart deleted event
        /// </summary>
        public static string CartDeletedEventName => "ECOMAIL_CART_DELETED";

        /// <summary>
        /// Gets a token to place app identifier in the tracking script
        /// </summary>
        public static string TrackingScriptAppId => "{AppId}";

        /// <summary>
        /// Gets a generic attribute name to hide general settings block on the plugin configuration page
        /// </summary>
        public static string HideGeneralBlock => $"{SystemName}.Page.HideGeneralBlock";

        /// <summary>
        /// Gets a generic attribute name to hide synchronization block on the plugin configuration page
        /// </summary>
        public static string HideSynchronizationBlock => $"{SystemName}.Page.HideSynchronizationBlock";

        /// <summary>
        /// Gets the name of the product feeds directory
        /// </summary>
        public static string FeedsDirectory => "EcomailProductFeeds";

        /// <summary>
        /// Gets a product feed filename
        /// </summary>
        /// <remarks>
        /// {0} : store Id
        /// </remarks>
        public static string FeedFileName => "product-feed-{0}.xml";

        #region API URLs

        /// <summary>
        /// Gets a base API URL
        /// </summary>
        public static string EcomailApiBaseUrl => "http://api2.ecomailapp.com";

        /// <summary>
        /// Gets a URL to add transaction data to account
        /// </summary>
        public static string AddTransactionDataToEcomailApiUrl => EcomailApiBaseUrl + "/tracker/transaction";

        /// <summary>
        /// Gets a URL to get contact lists
        /// </summary>
        public static string EcomailContactListApiUrl => EcomailApiBaseUrl + "/lists";

        /// <summary>
        /// Gets a URL to get contacts
        /// </summary>
        public static string GetContactListByIdApiUrl => EcomailApiBaseUrl + "/lists/{0}";

        /// <summary>
        /// Gets a URL to import contacts (subscribers) on bulk to account
        /// </summary>
        public static string SubscribeInBulkApiUrl => EcomailApiBaseUrl + "/lists/{0}/subscribe-bulk";

        /// <summary>
        /// Gets a URL to import contact (subscriber) on account
        /// </summary>
        public static string SubscribeToEcomailApiUrl => EcomailApiBaseUrl + "/lists/{0}/subscribe";

        /// <summary>
        /// Gets a URL to import contacts (subscribers) on account
        /// </summary>
        public static string SubscribeListOfEcomailApiUrl => EcomailApiBaseUrl + "/lists/{0}/subscribers";

        /// <summary>
        /// Gets a URL to unsubscriber from account
        /// </summary>
        public static string UnubscribeFromListEcomailApiUrl => EcomailApiBaseUrl + "/lists/{0}/unsubscribe";

        /// <summary>
        /// Gets a URL to track events
        /// </summary>
        public static string EcomailTrackEventApiUrl => EcomailApiBaseUrl + "/tracker/events";

        #endregion
    }
}