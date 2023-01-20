using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Ecomail.Models
{
    /// <summary>
    /// Represents create list model
    /// </summary>
    public record CreateListModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.List.New.Add")]
        public bool Add { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.List.New.ListName")]
        public string ListName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.List.New.FromName")]
        public string FromName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.List.New.FromEmail")]
        public string FromEmail { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Ecomail.Fields.List.New.ReplyTo")]
        public string ReplyTo { get; set; }
    }
}