using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Starcounter.Validation
{
    /// <summary>
    /// Used to configure validation in a Starcounter view-model. Usually an instance implementing this interface
    /// is obtained from the dependency injection container.
    /// </summary>
    public interface IValidatorBuilder
    {
        /// <summary>
        /// Sets the view-model that will be validated with this validator. If <paramref name="viewModel"/> implements <see cref="IValidatableObject"/>,
        /// then its <see cref="IValidatableObject.Validate"/> method will be called by <see cref="IValidator.ValidateAll"/>.
        /// Further calls to this method will override the previously set <see cref="ValidationResultsPresenter"/>. This method must be called at least once.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>The original builder object</returns>
        /// <remarks>This method changes and returns the original builder object</remarks>
        IValidatorBuilder WithViewModel(object viewModel);

        /// <summary>
        /// Registers a property that will be validated. If a property is not registered, it can't be later validated with <see cref="IValidator.Validate"/>
        /// and will not be validated by <see cref="IValidator.ValidateAll"/>.
        /// </summary>
        /// <param name="propertyName">Name of a C# property with public getter, belonging to the specified view-model.</param>
        /// <returns>The original builder object</returns>
        /// <remarks>This method changes and returns the original builder object</remarks>
        IValidatorBuilder AddProperty(string propertyName);

        /// <summary>
        /// Sets the <see cref="ValidationResultsPresenter"/> that will be used to present validation errors discovered by validator constructed by this builder.
        /// Further calls to this method will override the previously set <see cref="ValidationResultsPresenter"/>. This method must be called at least once.
        /// </summary>
        /// <param name="validationResultsPresenter">The <see cref="ValidationResultsPresenter"/> to use. It can not be null</param>
        /// <returns>The original builder object</returns>
        /// <remarks>This method changes and returns the original builder object</remarks>
        IValidatorBuilder WithResultsPresenter(ValidationResultsPresenter validationResultsPresenter);

        /// <summary>
        /// Returns names of all properties added so far with <see cref="AddProperty"/>.
        /// </summary>
        IEnumerable<string> Properties { get; }

        /// <summary>
        /// Builds the <see cref="IValidator"/>. If the <see cref="WithResultsPresenter"/> was never
        /// </summary>
        /// <returns>The new <see cref="IValidator"/> instance.</returns>
        /// <exception cref="InvalidOperationException"><see cref="WithResultsPresenter"/> or <see cref="WithViewModel"/> was never called.</exception>
        IValidator Build();
    }
}