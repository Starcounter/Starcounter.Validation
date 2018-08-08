using System;

namespace Starcounter.Validation
{
    /// <summary>
    /// Allows validating of Starcounter view-model and its properties. Obtain an instance with <see cref="IValidatorBuilderFactory"/>.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Validate <paramref name="value"/>, with regard to attributes of <paramref name="propertyName"/>.
        /// Any errors will be presented using associated <see cref="ValidationResultsPresenter"/>.
        /// </summary>
        /// <param name="propertyName">The name of the property that is used as a source of validation attributes.
        /// It must be registered in <see cref="IValidatorBuilder"/> beforehand.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>true if validation passes.</returns>
        /// <exception cref="InvalidOperationException">Property <paramref name="propertyName"/> was never added to this validator.</exception>
        bool Validate(string propertyName, object value);

        /// <summary>
        /// Validate all of the properties registered in this validator, with regard to their current values.
        /// It also validates all of the properties of sub-validators.
        /// Any errors will be presented using associated <see cref="ValidationResultsPresenter"/>.
        /// </summary>
        /// <returns>true if all the properties passed validation.</returns>
        bool ValidateAll();

        /// <summary>
        /// Create <see cref="IValidatorBuilder"/> that allows creating a sub-validator. <see cref="ValidateAll"/> only returns true if
        /// all the sub-validators also pass.
        /// </summary>
        /// <returns></returns>
        IValidatorBuilder CreateSubValidatorBuilder();
    }
}