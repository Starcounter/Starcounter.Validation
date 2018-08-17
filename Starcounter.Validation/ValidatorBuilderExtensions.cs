
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Starcounter.Validation
{
    public static class ValidatorBuilderExtensions
    {
        /// <summary>
        /// Registers properties that will be validated. If a property is not registered, it can't be later validated with <see cref="IValidator.Validate"/>
        /// and will not be validated by <see cref="IValidator.ValidateAll"/>.
        /// </summary>
        /// <param name="validatorBuilder">The <see cref="IValidatorBuilder"/> to add to.</param>
        /// <param name="properties">Name of a C# property with public getters, belonging to the specified view-model.</param>
        /// <returns>The original builder object</returns>
        /// <remarks>This method changes and returns the original builder object</remarks>
        public static IValidatorBuilder AddProperties(this IValidatorBuilder validatorBuilder, params string[] properties)
        {
            foreach (var property in properties)
            {
                validatorBuilder.AddProperty(property);
            }

            return validatorBuilder;
        }

        /// <summary>
        /// Sets the view-model and registers all its public properties that have at least one <see cref="ValidationAttribute"/> applied.
        /// </summary>
        /// <param name="validatorBuilder">The <see cref="IValidatorBuilder"/> to add to.</param>
        /// <param name="viewModel">The view-model, which type will be selected</param>
        /// <returns>The original builder object</returns>
        /// <remarks>This method changes and returns the original builder object</remarks>
        public static IValidatorBuilder WithViewModelAndAllProperties(this IValidatorBuilder validatorBuilder, object viewModel)
        {
            validatorBuilder.WithViewModel(viewModel);
            foreach (var property in viewModel.GetType()
                .GetProperties()
                .Where(property => property
                    .GetCustomAttributes<ValidationAttribute>()
                    .Any()))
            {
                validatorBuilder.AddProperty(property.Name);
            }

            return validatorBuilder;
        }


    }
}