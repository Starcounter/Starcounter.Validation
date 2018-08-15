
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
    }
}