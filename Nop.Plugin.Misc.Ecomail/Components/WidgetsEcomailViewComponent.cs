using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Nop.Plugin.Misc.Ecomail.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Ecomail.Components
{
    /// <summary>
    /// Represents view component to embed tracking script on pages
    /// </summary>
    [ViewComponent(Name = EcomailDefaults.TRACKING_VIEW_COMPONENT_NAME)]
    public class WidgetsEcomailViewComponent : NopViewComponent
    {
        #region Fields

        private readonly EcomailSettings _ecomailSettings;
        private readonly EcomailService _ecomailService;

        #endregion

        #region Ctor

        public WidgetsEcomailViewComponent(EcomailSettings ecomailSettings,
            EcomailService ecomailService)
        {
            _ecomailSettings = ecomailSettings;
            _ecomailService = ecomailService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            //ensure tracking is enabled
            if (!_ecomailSettings.UseTracking || string.IsNullOrEmpty(_ecomailSettings.AppId))
                return Content(string.Empty);

            await _ecomailService.SetEcomailIdAsync();

            //prepare tracking script
            var trackingScript = _ecomailSettings.TrackingScript?.Replace(EcomailDefaults.TrackingScriptAppId, _ecomailSettings.AppId);

            return new HtmlContentViewComponentResult(new HtmlString(trackingScript ?? string.Empty));
        }

        #endregion
    }
}