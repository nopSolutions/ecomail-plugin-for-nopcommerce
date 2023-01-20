using FluentValidation;
using Nop.Plugin.Misc.Ecomail.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.Ecomail.Validators
{
    /// <summary>
    /// Represents create list model validator
    /// </summary>
    public class CreateListModelValidator : BaseNopValidator<CreateListModel>
    {
        #region Ctor

        public CreateListModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.ListName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Ecomail.Fields.List.New.ListName.Required"))
                .When(model => model.Add);

            RuleFor(model => model.FromName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Ecomail.Fields.List.New.FromName.Required"))
                .When(model => model.Add);

            RuleFor(model => model.FromEmail)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Ecomail.Fields.List.New.FromEmail.Required"))
                .When(model => model.Add);

            RuleFor(model => model.FromEmail)
                .EmailAddress()
                .WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"))
                .When(model => model.Add);

            RuleFor(model => model.ReplyTo)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Ecomail.Fields.List.New.ReplyTo.Required"))
                .When(model => model.Add);

            RuleFor(model => model.ReplyTo)
                .EmailAddress()
                .WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"))
                .When(model => model.Add);
        }

        #endregion
    }
}