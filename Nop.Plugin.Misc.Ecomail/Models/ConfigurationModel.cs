using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Ecomail.Models
{
    /// <summary>
    /// Represents configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor

        public ConfigurationModel()
        {
            AvailableContactLists = new List<SelectListItem>();
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailableOrderEventTypes = new List<SelectListItem>();
            OrderStatuses = new List<int>();
            AvailableConsents = new List<SelectListItem>();
            NewList = new CreateListModel();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.ApiKey")]
        [DataType(DataType.Password)]
        public string ApiKey { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.UseTracking")]
        public bool UseTracking { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.TrackingScript")]
        public string TrackingScript { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.AppId")]
        public string AppId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.List")]
        public int ListId { get; set; }
        public IList<SelectListItem> AvailableContactLists { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.SyncSubscribersOnly")]
        public bool SyncSubscribersOnly { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.ImportOrdersOnSync")]
        public bool ImportOrdersOnSync { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.OrderStatuses")]
        public IList<int> OrderStatuses { get; set; }
        public IList<SelectListItem> AvailableOrderStatuses { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.OrderEventType")]
        public int OrderEventTypeId { get; set; }
        public IList<SelectListItem> AvailableOrderEventTypes { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.Consent")]
        public int ConsentId { get; set; }
        public IList<SelectListItem> AvailableConsents { get; set; }

        public string WebHookUrl { get; set; }
        public string FeedUrl { get; set; }

        public CreateListModel NewList { get; set; }

        #endregion
    }
}