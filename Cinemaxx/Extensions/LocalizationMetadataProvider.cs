using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;

namespace Cinemaxx.Extensions;

// Gives validation attributes an English message template so the data-annotations localizer can translate it,
// and localizes property names that have no [Display] attribute.
public class LocalizationMetadataProvider(IStringLocalizerFactory factory) : IValidationMetadataProvider, IDisplayMetadataProvider
{
    private readonly IStringLocalizer _localizer = factory.Create(typeof(SharedResource));

    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach (var attribute in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            if (attribute.ErrorMessage != null || attribute.ErrorMessageResourceName != null)
            {
                continue;
            }

            attribute.ErrorMessage = attribute switch
            {
                RequiredAttribute => "The {0} field is required.",
                StringLengthAttribute { MinimumLength: > 0 } => "The {0} must be between {2} and {1} characters long.",
                StringLengthAttribute => "The {0} must be at most {1} characters long.",
                RangeAttribute => "The {0} must be between {1} and {2}.",
                EmailAddressAttribute => "The {0} field is not a valid email address.",
                PhoneAttribute => "The {0} field is not a valid phone number.",
                UrlAttribute => "The {0} field is not a valid URL.",
                RegularExpressionAttribute => "The {0} field is not in the correct format.",
                _ => null
            };
        }
    }

    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        var hasDisplayName = context.Attributes.OfType<DisplayAttribute>().Any(a => a.Name != null);
        if (context.Key.Name != null && !hasDisplayName)
        {
            var name = context.Key.Name;
            context.DisplayMetadata.DisplayName = () => _localizer[name];
        }
    }
}
