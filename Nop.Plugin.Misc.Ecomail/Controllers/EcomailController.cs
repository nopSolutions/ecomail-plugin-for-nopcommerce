using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Ecomail.Services;

namespace Nop.Plugin.Misc.Ecomail.Controllers
{
    public class EcomailController : Controller
    {
        #region Fields

        private readonly EcomailService _ecomailService;

        #endregion

        #region Ctor

        public EcomailController(EcomailService ecomailService)
        {
            _ecomailService = ecomailService;
        }

        #endregion

        #region Methods

        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            await _ecomailService.HandleWebhookAsync(Request);
            return Ok();
        }

        public async Task<IActionResult> ProductFeed()
        {
            var feedPath = await _ecomailService.GenerateFeedAsync();
            if (!string.IsNullOrEmpty(feedPath))
                return PhysicalFile(feedPath, MimeTypes.ApplicationXml);

            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        #endregion
    }
}