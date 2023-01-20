using FluentValidation;
using Nop.Plugin.Misc.Ecomail.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.Ecomail.Validators
{
    /// <summary>
    /// Represents configuration model validator
    /// </summary>
    public class ConfigurationModelValidator : BaseNopValidator<ConfigurationModel>
    {
        #region Ctor

        public ConfigurationModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.ApiKey)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Ecomail.Fields.ApiKey.Required"));

            RuleFor(model => model.AppId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Ecomail.Fields.AppId.Required"))
                .When(model => model.UseTracking);

            RuleFor(model => model.TrackingScript)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Ecomail.Fields.TrackingScript.Required"))
                .When(model => model.UseTracking);
        }

        #endregion
    }
}