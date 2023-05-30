using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Ecomail.Domain;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.Ecomail
{
    /// <summary>
    /// Represents Ecomail plugin
    /// </summary>
    public class EcomailPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public EcomailPlugin(IActionContextAccessor actionContextAccessor,
            ILocalizationService localizationService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            WidgetSettings widgetSettings)
        {
            _actionContextAccessor = actionContextAccessor;
            _localizationService = localizationService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.BodyStartHtmlTagAfter });
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            return EcomailDefaults.TRACKING_VIEW_COMPONENT_NAME;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext)
                .RouteUrl(EcomailDefaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            var ecomailTrackingScript =
@$"<!-- Ecomail starts --> 
<script type='text/javascript'> 
    ;(function(p,l,o,w,i,n,g){{if(!p[i]){{p.GlobalSnowplowNamespace=p.GlobalSnowplowNamespace||[]; p.GlobalSnowplowNamespace.push(i);p[i]=function(){{(p[i].q=p[i].q||[]).push(arguments)}};p[i].q=p[i].q||[];n=l.createElement(o);g=l.getElementsByTagName(o)[0];n.async=1; n.src=w;g.parentNode.insertBefore(n,g)}}}}(window,document,'script','//d70shl7vidtft.cloudfront.net/ecmtr-2.4.2.js','ecotrack')); 
        window.ecotrack('newTracker', 'cf', 'd2dpiwfhf3tz0r.cloudfront.net', {{ /* Initialise a tracker */   
        appId: '{EcomailDefaults.TrackingScriptAppId}'
    }}); 
    window.ecotrack('setUserIdFromLocation', 'ecmid'); 
    window.ecotrack('trackPageView'); 
</script> 
<!-- Ecomail stops -->";

            var settings = new EcomailSettings
            {
                UseTracking = true,
                TrackingScript = ecomailTrackingScript,
                SyncSubscribersOnly = true,
                ExportContactsOnSync = false,
                ImportOrdersOnSync = false,
                LogRequests = false,
                LogTrackingErrors = false,
                SyncPageSize = 300,
                RebuildFeedXmlAfterHours = 48,
                OrderStatuses = new() { (int)OrderStatus.Complete },
                OrderEventType = OrderEventType.Placed,
                RequestTimeout = EcomailDefaults.RequestTimeout
            };

            await _settingService.SaveSettingAsync(settings);

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(EcomailDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(EcomailDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Misc.Ecomail.Credentials"] = "Credentials",

                ["Plugins.Misc.Ecomail.Fields.ApiKey"] = "API key",
                ["Plugins.Misc.Ecomail.Fields.ApiKey.Hint"] = "Enter the Ecomail integration API key.",
                ["Plugins.Misc.Ecomail.Fields.ApiKey.Required"] = "API key is required",
                ["Plugins.Misc.Ecomail.Fields.UseTracking"] = "Use tracking",
                ["Plugins.Misc.Ecomail.Fields.UseTracking.Hint"] = "Determine whether to use tracking to get statistics with Ecomail, such as the behavior of individual subsribers on your website, including purchases made, movements on your site and subsequent segmentation.",
                ["Plugins.Misc.Ecomail.Fields.TrackingScript"] = "Tracking code",
                ["Plugins.Misc.Ecomail.Fields.TrackingScript.Hint"] = "Enter tracking code generated by Ecomail here. You can use a tracking code to track user behavior on your site after they click through an email. {AppId} will be dynamically replaced with App ID specified below.",
                ["Plugins.Misc.Ecomail.Fields.TrackingScript.Required"] = "Tracking code is required",
                ["Plugins.Misc.Ecomail.Fields.AppId"] = "App ID",
                ["Plugins.Misc.Ecomail.Fields.AppId.Hint"] = "Enter the identifier (name) of your account. This name is the same as the beginning of your account address. So, for example, the account 'foo.ecomailapp.cz' will have the name 'foo'.",
                ["Plugins.Misc.Ecomail.Fields.AppId.Required"] = "App ID is required",
                ["Plugins.Misc.Ecomail.Fields.List"] = "Contact list",
                ["Plugins.Misc.Ecomail.Fields.List.Hint"] = "Select the Ecomail list to synchronize subscribers/customers from your store.",
                ["Plugins.Misc.Ecomail.Fields.SyncSubscribersOnly"] = "Synchronize subscribers only",
                ["Plugins.Misc.Ecomail.Fields.SyncSubscribersOnly.Hint"] = "Determine whether to synchronize only the contacts who have opted-in for the newsletter. When disabled, all customers who made an order in the store will also be synchronized.",
                ["Plugins.Misc.Ecomail.Fields.ImportOrdersOnSync"] = "Synchronize orders",
                ["Plugins.Misc.Ecomail.Fields.ImportOrdersOnSync.Hint"] = "Determine whether to import orders from the store to Ecomail account during synchronization and in real time.",
                ["Plugins.Misc.Ecomail.Fields.OrderStatuses"] = "Order statuses",
                ["Plugins.Misc.Ecomail.Fields.OrderStatuses.Hint"] = "Specify statuses of orders to import during synchronization.",
                ["Plugins.Misc.Ecomail.Fields.OrderEventType"] = "Order event type",
                ["Plugins.Misc.Ecomail.Fields.OrderEventType.Hint"] = "Specify the type of order event at which to import the order in real time.",
                ["Plugins.Misc.Ecomail.Fields.Consent"] = "GDPR Consent",
                ["Plugins.Misc.Ecomail.Fields.Consent.Hint"] = "Select GDPR consent that the subscriber must accept in order to subscribe to the newsletter. Leave it blank to sync contacts anyway.",

                ["Plugins.Misc.Ecomail.Fields.List.New"] = "Create new list",
                ["Plugins.Misc.Ecomail.Fields.List.New.Add"] = "Add new list",
                ["Plugins.Misc.Ecomail.Fields.List.New.Add.Hint"] = "Check to add new contact list.",
                ["Plugins.Misc.Ecomail.Fields.List.New.ListName"] = "List name",
                ["Plugins.Misc.Ecomail.Fields.List.New.ListName.Hint"] = "Enter name of the Ecomail contact list.",
                ["Plugins.Misc.Ecomail.Fields.List.New.ListName.Required"] = "List name is required",
                ["Plugins.Misc.Ecomail.Fields.List.New.FromName"] = "Sender name",
                ["Plugins.Misc.Ecomail.Fields.List.New.FromName.Hint"] = "Enter sender name of the email.",
                ["Plugins.Misc.Ecomail.Fields.List.New.FromName.Required"] = "Sender name is required",
                ["Plugins.Misc.Ecomail.Fields.List.New.FromEmail"] = "Sender email",
                ["Plugins.Misc.Ecomail.Fields.List.New.FromEmail.Hint"] = "Enter email of the sender.",
                ["Plugins.Misc.Ecomail.Fields.List.New.FromEmail.Required"] = "Sender email is required",
                ["Plugins.Misc.Ecomail.Fields.List.New.ReplyTo"] = "Reply to email",
                ["Plugins.Misc.Ecomail.Fields.List.New.ReplyTo.Hint"] = "Enter email address that subscriber will reply.",
                ["Plugins.Misc.Ecomail.Fields.List.New.ReplyTo.Required"] = "Reply email is required",

                ["Plugins.Misc.Ecomail.Sync"] = "Contacts",
                ["Plugins.Misc.Ecomail.Sync.Button"] = "Sync now",
                ["Plugins.Misc.Ecomail.Sync.Completed"] = "Synchronization completed",
                ["Plugins.Misc.Ecomail.Sync.Start"] = "Start synchronization",
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(EcomailDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(EcomailDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }
            await _settingService.DeleteSettingAsync<EcomailSettings>();

            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.Ecomail");

            await base.UninstallAsync();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => true;

        #endregion
    }
}