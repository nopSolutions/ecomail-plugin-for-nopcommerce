using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.Ecomail.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(name: EcomailDefaults.ConfigurationRouteName,
                pattern: "Admin/EcomailAdmin/Configure",
                defaults: new { controller = "EcomailAdmin", action = "Configure", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: EcomailDefaults.WebhookRoute,
                pattern: "Plugins/Ecomail/Webhook",
                defaults: new { controller = "Ecomail", action = "Webhook" });

            endpointRouteBuilder.MapControllerRoute(name: EcomailDefaults.ProductFeedRoute,
                pattern: "ecomail-product-feed.xml",
                defaults: new { controller = "Ecomail", action = "ProductFeed" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}