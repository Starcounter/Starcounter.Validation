using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Starcounter.Validation
{
    public class LocalizationValidationAttributeAdapter : IValidationAttributeAdapter
    {
        private readonly Func<Type, IStringLocalizer> _localizerProvider;

        public LocalizationValidationAttributeAdapter(IOptions<StarcounterValidationOptions> options, IStringLocalizerFactory localizerFactory)
        {
            _localizerProvider = (type) => options.Value.DataAnnotationLocalizerProvider(type, localizerFactory);
        }
        public ValidationAttribute Adapt(ValidationAttribute original, Type viewModelType)
        {
            original.ErrorMessage = _localizerProvider(viewModelType)[original.ErrorMessage];
            return original;
        }
    }

    /// <summary>
    /// Sets up default options for <see cref="StarcounterValidationOptions" />.
    /// </summary>
    public class StarcounterValidationOptionsSetup : IConfigureOptions<StarcounterValidationOptions>
    {
        /// <inheritdoc />
        public void Configure(StarcounterValidationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.DataAnnotationLocalizerProvider = (viewModelType, stringLocalizerFactory) => stringLocalizerFactory.Create(viewModelType);
        }
    }

    /// <summary>
    /// Provides programmatic configuration for DataAnnotations localization.
    /// </summary>
    public class StarcounterValidationOptions
    {
        /// <summary>
        /// The delegate to invoke for creating <see cref="IStringLocalizer" />.
        /// </summary>
        public Func<Type, IStringLocalizerFactory, IStringLocalizer> DataAnnotationLocalizerProvider;
    }



}