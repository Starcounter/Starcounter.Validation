using System;
using System.ComponentModel.DataAnnotations;

namespace Starcounter.Validation
{
    public interface IValidationAttributeAdapter
    {
        ValidationAttribute Adapt(ValidationAttribute original, Type viewModelType);
    }
}