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
            AvailableConsents = new List<SelectListItem>();
            NewList = new CreateListModel();
        }

        #endregion

        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.ApiKey")]
        [DataType(DataType.Password)]
        public string ApiKey { get; set; }
        public bool ApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.UseTracking")]
        public bool UseTracking { get; set; }
        public bool UseTracking_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.TrackingScript")]
        public string TrackingScript { get; set; }
        public bool TrackingScript_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.AppId")]
        public string AppId { get; set; }
        public bool AppId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.List")]
        public int ListId { get; set; }
        public bool ListId_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableContactLists { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.Consent")]
        public int ConsentId { get; set; }
        public IList<SelectListItem> AvailableConsents { get; set; }

        public string WebHookUrl { get; set; }
        public string FeedUrl { get; set; }

        public bool HideGeneralBlock { get; set; }

        public bool HideSynchronizationBlock { get; set; }

        public CreateListModel NewList { get; set; }

        #endregion
    }
}