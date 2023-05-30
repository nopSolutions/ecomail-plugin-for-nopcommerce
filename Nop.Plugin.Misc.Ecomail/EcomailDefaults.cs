using Nop.Core;

namespace Nop.Plugin.Misc.Ecomail
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public static class EcomailDefaults
    {
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
        /// Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.Misc.Ecomail.Configure";

        /// <summary>
        /// Gets a name of the webhook route
        /// </summary>
        public static string WebhookRoute => "Plugin.Misc.Ecomail.Webhook";

        /// <summary>
        /// Gets a name of the product feed route
        /// </summary>
        public static string ProductFeedRoute => "Plugin.Misc.Ecomail.ProductFeed";

        /// <summary>
        /// Gets a key of the attribute to store shopping cart identifier
        /// </summary>
        public static string ShoppingCartGuidAttribute => $"{SystemName}.ShoppingCart.Guid";

        /// <summary>
        /// Gets a key of the attribute to store shopping cart
        /// </summary>
        public static string BasketActionAttribute => "Basket";

        /// <summary>
        /// Gets a key of the attribute to store customer email identifier
        /// </summary>
        public static string CustomerEcomailIdAttribute => $"{SystemName}.Customer.EcomailId";

        /// <summary>
        /// Gets a token to place app identifier in the tracking script
        /// </summary>
        public static string TrackingScriptAppId => "{AppId}";

        /// <summary>
        /// Gets a custom field to mark a contact as newsletter subscriber
        /// </summary>
        public static (string Name, string Value) SubscriberCustomField => ("IsSubscribed", "YES");

        /// <summary>
        /// Gets the name of the product feeds directory
        /// </summary>
        public static string FeedsDirectory => "EcomailProductFeeds";

        /// <summary>
        /// Gets a product feed filename
        /// </summary>
        public static string FeedFileName => "product-feed.xml";

        /// <summary>
        /// Gets a period (in seconds) before the request times out
        /// </summary>
        public static int RequestTimeout => 30;

        #region API URLs

        /// <summary>
        /// Gets a base API URL
        /// </summary>
        public static string BaseApiUrl => "http://api2.ecomailapp.com";

        /// <summary>
        /// Gets a URL to add transaction data to account
        /// </summary>
        public static string AddTransactionDataApiUrl => BaseApiUrl + "/tracker/transaction";

        /// <summary>
        /// Gets a URL to add transactions to account
        /// </summary>
        public static string AddTransactionsApiUrl => BaseApiUrl + "/tracker/transaction-bulk";

        /// <summary>
        /// Gets a URL to track events
        /// </summary>
        public static string TrackEventApiUrl => BaseApiUrl + "/tracker/events";

        /// <summary>
        /// Gets a URL to get contact lists
        /// </summary>
        public static string ListsApiUrl => BaseApiUrl + "/lists";

        /// <summary>
        /// Gets a URL to get contacts
        /// </summary>
        public static string GetListApiUrl => BaseApiUrl + "/lists/{0}";

        /// <summary>
        /// Gets a URL to import contacts (subscribers) on bulk to account
        /// </summary>
        public static string SubscribeInBulkApiUrl => BaseApiUrl + "/lists/{0}/subscribe-bulk";

        /// <summary>
        /// Gets a URL to import contact (subscriber) on account
        /// </summary>
        public static string SubscribeApiUrl => BaseApiUrl + "/lists/{0}/subscribe";

        /// <summary>
        /// Gets a URL to unsubscriber from account
        /// </summary>
        public static string UnubscribeApiUrl => BaseApiUrl + "/lists/{0}/unsubscribe";

        /// <summary>
        /// Gets a URL to import contacts (subscribers) on account
        /// </summary>
        public static string GetSubscribersApiUrl => BaseApiUrl + "/lists/{0}/subscribers";

        /// <summary>
        /// Gets a URL to get single contact (subscriber) on account
        /// </summary>
        public static string GetSubscriberApiUrl => BaseApiUrl + "/subscribers/{0}";

        #endregion
    }
}