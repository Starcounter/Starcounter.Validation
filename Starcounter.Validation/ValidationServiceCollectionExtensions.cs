using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Starcounter.Validation
{
    public static class ValidationServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="IValidatorBuilder"/> to the <see cref="IServiceCollection"/>, allowing for validation of Starcounter view-models.
        /// </summary>
        /// <param name="serviceCollection">The collection to add to.</param>
        /// <param name="setupAction">The configuration for validation.</param>
        /// <returns>The original service collection</returns>
        /// <remarks>This method changes and returns the original service collection</remarks>
        public static IServiceCollection AddStarcounterValidation(this IServiceCollection serviceCollection, Action<StarcounterValidationOptions> setupAction = null)
        {
            serviceCollection.TryAddTransient<IValidatorBuilder, ValidatorBuilder>();
            if (setupAction != null)
            {
                serviceCollection.Configure(setupAction);
            }
            else
            {
                serviceCollection.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<StarcounterValidationOptions>, StarcounterValidationOptionsSetup>());
            }

            return serviceCollection;
        }

    }
}