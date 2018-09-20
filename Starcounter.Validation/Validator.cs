using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Starcounter.Validation
{
    /// <summary>
    /// The default implementation of <see cref="IValidator"/>. Instances of this class are usually constructed by <see cref="ValidatorBuilder"/>
    /// </summary>
    public sealed class Validator : IValidator
    {
        public delegate IValidatorBuilder ValidatorBuilderFactory(ValidatorBuilder.ValidatorBuildHandler onBuild, ValidatorBuilder.ValidatorDisposeHandler onDispose);

        private readonly ValidationResultsPresenter _validationResultsPresenter;
        private readonly IDictionary<string, PropertyValidationData> _properties;
        private readonly List<IValidator> _subValidators = new List<IValidator>();
        private readonly object _viewModel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ValidatorBuilderFactory _validatorBuilderFactory;
        private readonly ValidatorBuilder.ValidatorDisposeHandler _validatorDisposeHandler;

        private bool _isDisposed = false;

        public Validator(IServiceProvider serviceProvider,
            ValidationResultsPresenter validationResultsPresenter,
            IDictionary<string, PropertyValidationData> properties,
            object viewModel,
            ValidatorBuilderFactory validatorBuilderFactory,
            ValidatorBuilder.ValidatorDisposeHandler validatorDisposeHandler)
        {
            _serviceProvider = serviceProvider;
            _validatorBuilderFactory = validatorBuilderFactory;
            _validatorDisposeHandler = validatorDisposeHandler;
            _viewModel = viewModel;
            _validationResultsPresenter = validationResultsPresenter;
            _properties = properties;
        }

        /// <inheritdoc />
        public bool Validate(string propertyName, object value)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            CheckDisposed();
            IReadOnlyCollection<ValidationAttribute> attributes;
            try
            {
                attributes = _properties[propertyName].Attributes;
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException(string.Format(Strings.Validator_PropertyNeverAdded, propertyName));
            }

            return Validate(propertyName, value, attributes);
        }

        /// <inheritdoc />
        public bool ValidateAll()
        {
            CheckDisposed();
            var areAllPropertiesValid = true;
            foreach (var entry in _properties)
            {
                areAllPropertiesValid &= Validate(entry.Key, entry.Value.Getter(), entry.Value.Attributes);
            }

            foreach (var subValidator in _subValidators)
            {
                areAllPropertiesValid &= subValidator.ValidateAll();
            }

            return areAllPropertiesValid;
        }

        /// <inheritdoc />
        public IValidatorBuilder CreateSubValidatorBuilder()
        {
            CheckDisposed();
            return _validatorBuilderFactory(AddSubValidator, RemoveSubValidator);
        }

        /// <summary>
        /// If this instance is a sub-validator, then it gets detached from its parent.
        /// Otherwise, nothing happens.
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;
            _validatorDisposeHandler?.Invoke(this);
        }

        private void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(typeof(Validator).FullName);
            }
        }

        private void AddSubValidator(IValidator validator)
        {
            _subValidators.Add(validator);
        }

        private void RemoveSubValidator(IValidator validator)
        {
            _subValidators.Remove(validator);
        }

        private bool Validate(string propertyName, object value, IReadOnlyCollection<ValidationAttribute> attributes)
        {
            var validationContext = new ValidationContext(_viewModel)
            {
                MemberName = propertyName,
            };
            validationContext.InitializeServiceProvider(_serviceProvider.GetService);
            var errors = attributes
                    .Select(att => att.GetValidationResult(value, validationContext))
                    .Where(result => result != ValidationResult.Success)
                    .ToList();
            _validationResultsPresenter(propertyName, errors.Select(result => result.ErrorMessage));

            return !errors.Any();
        }

        /// <summary>
        /// Stores information that <see cref="Validator"/> needs to know about a specific property of a specific object.
        /// </summary>
        public class PropertyValidationData
        {
            /// <summary>
            /// A collection of validation attributes associated with the property.
            /// </summary>
            public IReadOnlyCollection<ValidationAttribute> Attributes { get; set; }

            /// <summary>
            /// A delegate that, when called will return the value of the property.
            /// </summary>
            public Func<object> Getter { get; set; }
        }
    }
}