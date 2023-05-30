using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Ecomail.Domain;
using Nop.Plugin.Misc.Ecomail.Domain.Api;
using Nop.Plugin.Misc.Ecomail.Models;
using Nop.Plugin.Misc.Ecomail.Services;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Gdpr;
using Nop.Services.Localization;
using Nop.Services.Messages;
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
        private readonly EcomailSettings _ecomailSettings;
        private readonly IGdprService _gdprService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public EcomailAdminController(EcomailService ecomailService,
            EcomailSettings ecomailSettings,
            IGdprService gdprService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IWebHelper webHelper)
        {
            _ecomailService = ecomailService;
            _ecomailSettings = ecomailSettings;
            _gdprService = gdprService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            var model = new ConfigurationModel
            {
                ApiKey = _ecomailSettings.ApiKey,
                UseTracking = _ecomailSettings.UseTracking,
                TrackingScript = _ecomailSettings.TrackingScript,
                AppId = _ecomailSettings.AppId,
                ListId = _ecomailSettings.ListId,
                SyncSubscribersOnly = _ecomailSettings.SyncSubscribersOnly,
                ImportOrdersOnSync = _ecomailSettings.ImportOrdersOnSync,
                OrderStatuses = _ecomailSettings.OrderStatuses ?? new(),
                OrderEventTypeId = (int)_ecomailSettings.OrderEventType,
                ConsentId = _ecomailSettings.ConsentId
            };

            //get available lists to synchronize contacts
            if (!string.IsNullOrEmpty(_ecomailSettings.ApiKey))
            {
                var (lists, error) = await _ecomailService.GetListsAsync();
                if (!string.IsNullOrEmpty(error))
                    _notificationService.ErrorNotification(error);

                model.AvailableContactLists = lists?.Select(list => new SelectListItem(list.Name, list.Id)).ToList() ?? new();
            }
            model.AvailableContactLists.Add(new SelectListItem(await _localizationService.GetResourceAsync("Admin.Common.Select"), "0"));

            model.AvailableOrderStatuses = (await OrderStatus.Pending.ToSelectListAsync(false, useLocalization: false))
                .Select(item => new SelectListItem(item.Text, item.Value, int.TryParse(item.Value, out var value) && model.OrderStatuses.Contains(value)))
                .ToList();

            model.AvailableOrderEventTypes = (await OrderEventType.Placed.ToSelectListAsync(false, useLocalization: false))
                .Select(item => new SelectListItem(item.Text, item.Value))
                .ToList();

            model.AvailableConsents = (await _gdprService.GetAllConsentsAsync())
                .Select(consent => new SelectListItem(CommonHelper.EnsureMaximumLength(consent.Message, 30, "..."), consent.Id.ToString()))
                .ToList();
            model.AvailableConsents.Add(new SelectListItem(await _localizationService.GetResourceAsync("Admin.Common.EmptyItemText"), "0"));

            model.WebHookUrl = Url.RouteUrl(EcomailDefaults.WebhookRoute, null, _webHelper.GetCurrentRequestProtocol()).ToLowerInvariant();

            return View("~/Plugins/Misc.Ecomail/Views/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            _ecomailSettings.ApiKey = model.ApiKey;
            _ecomailSettings.UseTracking = model.UseTracking;
            _ecomailSettings.TrackingScript = model.TrackingScript;
            _ecomailSettings.AppId = model.AppId;
            await _settingService.SaveSettingAsync(_ecomailSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("saveSync")]
        public async Task<IActionResult> SaveSynchronization(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            _ecomailSettings.ListId = model.ListId;
            _ecomailSettings.SyncSubscribersOnly = model.SyncSubscribersOnly;
            _ecomailSettings.ImportOrdersOnSync = model.ImportOrdersOnSync;
            //_ecomailSettings.OrderStatuses = model.OrderStatuses?.ToList();
            //_ecomailSettings.OrderEventType = (OrderEventType)model.OrderEventTypeId;
            _ecomailSettings.ConsentId = model.ConsentId;
            await _settingService.SaveSettingAsync(_ecomailSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("sync")]
        public async Task<IActionResult> Synchronization()
        {
            var messages = await _ecomailService.SynchronizeAsync();
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