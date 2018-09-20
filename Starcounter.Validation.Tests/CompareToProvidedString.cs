using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace Starcounter.Validation.Tests
{
    public class CompareToProvidedString : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            var providedValue = validationContext.GetService<string>();
            return Equals(value, providedValue)
                ? ValidationResult.Success
                : new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        public override bool RequiresValidationContext => true;
    }
}