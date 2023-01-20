using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Misc.Ecomail.Domains.Api;
using Nop.Plugin.Misc.Ecomail.Models;
using Nop.Plugin.Misc.Ecomail.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Gdpr;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Ecomail.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    public class EcomailAdminController : BasePluginController
    {
        #region Fields

        private readonly EcomailService _ecomailService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public EcomailAdminController(EcomailService ecomailService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            IWorkContext workContext)
        {
            _ecomailService = ecomailService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _storeService = storeService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ecomailSettings = await _settingService.LoadSettingAsync<EcomailSettings>(storeId);

            var model = new ConfigurationModel
            {
                ApiKey = ecomailSettings.ApiKey,
                UseTracking = ecomailSettings.UseTracking,
                TrackingScript = ecomailSettings.TrackingScript,
                AppId = ecomailSettings.AppId,
                ListId = ecomailSettings.ListId,
                ConsentId = ecomailSettings.ConsentId,
                ActiveStoreScopeConfiguration = storeId
            };

            if (storeId > 0)
            {
                model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(ecomailSettings, settings => settings.ApiKey, storeId);
                model.UseTracking_OverrideForStore = await _settingService.SettingExistsAsync(ecomailSettings, settings => settings.UseTracking, storeId);
                model.TrackingScript_OverrideForStore = await _settingService.SettingExistsAsync(ecomailSettings, settings => settings.TrackingScript, storeId);
                model.AppId_OverrideForStore = await _settingService.SettingExistsAsync(ecomailSettings, settings => settings.AppId, storeId);
                model.ListId_OverrideForStore = await _settingService.SettingExistsAsync(ecomailSettings, settings => settings.ListId, storeId);
            }

            //get available lists to synchronize contacts
            if (!string.IsNullOrEmpty(ecomailSettings.ApiKey))
            {
                var (lists, error) = await _ecomailService.GetListsAsync(ecomailSettings.ApiKey);
                if (!string.IsNullOrEmpty(error))
                    _notificationService.ErrorNotification(error);

                model.AvailableContactLists = lists?.Select(list => new SelectListItem(list.Name, list.Id)).ToList() ?? new();
            }
            model.AvailableContactLists.Add(new SelectListItem(await _localizationService.GetResourceAsync("Admin.Common.Select"), "0"));
            
            model.AvailableConsents = (await _gdprService.GetAllConsentsAsync())
                .Select(consent => new SelectListItem(CommonHelper.EnsureMaximumLength(consent.Message, 30, "..."), consent.Id.ToString()))
                .ToList();
            model.AvailableConsents.Add(new SelectListItem(await _localizationService.GetResourceAsync("Admin.Common.EmptyItemText"), "0"));

            var store = storeId > 0
                ? await _storeService.GetStoreByIdAsync(storeId)
                : await _storeContext.GetCurrentStoreAsync();
            model.WebHookUrl = $"{store.Url.TrimEnd('/')}{Url.RouteUrl(EcomailDefaults.WebhookRoute)}".ToLowerInvariant();
            model.FeedUrl = $"{store.Url.TrimEnd('/')}{Url.RouteUrl(EcomailDefaults.ProductFeedRoute)}".ToLowerInvariant();

            var customer = await _workContext.GetCurrentCustomerAsync();
            model.HideGeneralBlock = await _genericAttributeService.GetAttributeAsync<bool>(customer, EcomailDefaults.HideGeneralBlock);
            model.HideSynchronizationBlock = await _genericAttributeService.GetAttributeAsync<bool>(customer, EcomailDefaults.HideSynchronizationBlock);

            return View("~/Plugins/Misc.Ecomail/Views/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ecomailSettings = await _settingService.LoadSettingAsync<EcomailSettings>(storeId);

            ecomailSettings.ApiKey = model.ApiKey;
            ecomailSettings.UseTracking = model.UseTracking;
            ecomailSettings.TrackingScript = model.TrackingScript;
            ecomailSettings.AppId = model.AppId;

            await _settingService.SaveSettingOverridablePerStoreAsync(ecomailSettings, settings => settings.ApiKey, model.ApiKey_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ecomailSettings, settings => settings.UseTracking, model.UseTracking_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ecomailSettings, settings => settings.TrackingScript, model.TrackingScript_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(ecomailSettings, settings => settings.AppId, model.AppId_OverrideForStore, storeId, false);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("saveSync")]
        public async Task<IActionResult> SaveSynchronization(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ecomailSettings = await _settingService.LoadSettingAsync<EcomailSettings>(storeId);

            ecomailSettings.ListId = model.ListId;
            ecomailSettings.ConsentId = model.ConsentId;
            await _settingService.SaveSettingOverridablePerStoreAsync(ecomailSettings, settings => settings.ListId, model.ListId_OverrideForStore, storeId, false);
            await _settingService.SaveSettingAsync(ecomailSettings, settings => settings.ConsentId, 0, false);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("sync")]
        public async Task<IActionResult> Synchronization()
        {
            //synchronize contacts of selected store
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var messages = await _ecomailService.SynchronizeAsync(false, storeId);
            foreach (var message in messages)
            {
                _notificationService.Notification(message.Type, message.Message, false);
            }
            if (!messages.Any(message => message.Type == NotifyType.Error))
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.Ecomail.Sync.Completed"));

            return await Configure();
        }

        [HttpPost]
        public async Task<IActionResult> CreateContactList(CreateListModel model)
        {
            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            var request = new CreateListRequest
            {
                ListName = model.ListName,
                FromName = model.FromName,
                FromEmail = model.FromEmail,
                ReplyTo = model.ReplyTo
            };
            var (contactList, error) = await _ecomailService.CreateListAsync(request);
            if (!string.IsNullOrEmpty(error))
                return ErrorJson(error);

            return Json(new { result = contactList });
        }

        #endregion
    }
}