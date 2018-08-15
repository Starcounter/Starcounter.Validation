using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Starcounter.Validation
{
    /// <summary>
    /// The default implementation of <see cref="IValidatorBuilder"/>
    /// </summary>
    public class ValidatorBuilder : IValidatorBuilder
    {
        /// <summary>
        /// Responds to a validator being created by a builder.
        /// </summary>
        /// <param name="validator">The newly created validator.</param>
        public delegate void ValidatorBuildHandler(IValidator validator);

        private static readonly MethodInfo CreateGetterMethodInfo =
            typeof(ValidatorBuilder)
                .GetMethod(nameof(CreateGetter), BindingFlags.NonPublic | BindingFlags.Static);
        
        private readonly IValidationAttributeAdapter _validationAttributeAdapter;
        private readonly ValidatorBuildHandler _validatorBuildHandler;
        private readonly IDictionary<string, Validator.PropertyValidationData> _properties = new Dictionary<string, Validator.PropertyValidationData>();

        private object _viewModel;
        private ValidationResultsPresenter _validationResultsPresenter;
        private Type _viewModelType;

        /// <summary>
        /// Constructs a new instance of <see cref="ValidatorBuilder"/>.
        /// </summary>
        /// <param name="validationAttributeAdapter">Used when scanning properties for <see cref="ValidationAttribute"/>. If this is null, no adaptation will occur.</param>
        public ValidatorBuilder(IValidationAttributeAdapter validationAttributeAdapter = null) : this(validationAttributeAdapter, null)
        {
        }

        /// <summary>
        /// Constructs a new instance of <see cref="ValidatorBuilder"/>. This constructor is not meant to be called directly by the app developer,
        /// but rather be used by <see cref="Validator.AddSubValidator"/>
        /// </summary>
        /// <param name="validationAttributeAdapter">Used when scanning properties for <see cref="ValidationAttribute"/>. If this is null, no adaptation will occur.</param>
        /// <param name="validatorBuildHandler">Called after <see cref="Build"/> is invoked.</param>
        public ValidatorBuilder(IValidationAttributeAdapter validationAttributeAdapter, ValidatorBuildHandler validatorBuildHandler)
        {
            _validationAttributeAdapter = validationAttributeAdapter;
            _validatorBuildHandler = validatorBuildHandler;
        }

        /// <inheritdoc/>
        public IValidatorBuilder WithViewModel(object viewModel)
        {
            _viewModel = viewModel;
            // cache it here instead of evaluating each time AddProperty is called
            _viewModelType = viewModel.GetType();
            return this;
        }

        /// <inheritdoc />
        public IValidatorBuilder AddProperty(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (_properties.ContainsKey(propertyName))
            {
                throw new InvalidOperationException(
                    string.Format(Strings.ValidatorBuilder_PropertyAlreadyAdded, propertyName));
            }

            var property = _viewModelType.GetProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(string.Format(Strings.ValidatorBuilder_ViewModelMissingProperty,
                    _viewModel, propertyName));
            }

            if (!property.CanRead)
            {
                throw new InvalidOperationException(string.Format(Strings.ValidatorBuilder_PropertyGetterMissing,
                    _viewModelType, propertyName));
            }

            var propertyData = new Validator.PropertyValidationData
            {
                Attributes = GetValidationAttributesFromProperty(property),
                Getter = (Func<object>) CreateGetterMethodInfo
                    // CreateGetter has no generic constraints, so this should never fail
                    .MakeGenericMethod(_viewModelType, property.PropertyType)
                    .Invoke(null, new[] {_viewModel, property })
            };

            _properties[propertyName] = propertyData;

            return this;
        }

        /// <inheritdoc />
        public IValidatorBuilder WithResultsPresenter(ValidationResultsPresenter validationResultsPresenter)
        {
            _validationResultsPresenter = validationResultsPresenter;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<string> Properties => _properties.Keys;

        /// <inheritdoc />
        public IValidator Build()
        {
            if (_validationResultsPresenter == null)
            {
                throw new InvalidOperationException(string.Format(Strings.ValidatorBuilder_ResultsPresenterMissing, nameof(WithResultsPresenter)));
            }
            if (_viewModel == null)
            {
                throw new InvalidOperationException(string.Format(Strings.ValidatorBuilder_ViewModelMissing, nameof(WithViewModel)));
            }

            var validator = new Validator(_validationResultsPresenter, _properties, _viewModel, CloneWithBuildHandler);
            _validatorBuildHandler?.Invoke(validator);

            return validator;
        }

        private List<ValidationAttribute> GetValidationAttributesFromProperty(PropertyInfo property)
        {
            var attributes = property.GetCustomAttributes<ValidationAttribute>();
            if (_validationAttributeAdapter != null)
            {
                attributes = attributes.Select(original => _validationAttributeAdapter.Adapt(original, _viewModelType));
            }
            return attributes.ToList();
        }

        private IValidatorBuilder CloneWithBuildHandler(ValidatorBuildHandler buildHandler)
        {
            return new ValidatorBuilder(_validationAttributeAdapter, buildHandler);
        }

        private static Func<TProperty> CreateGetter<TViewModel, TProperty>(TViewModel viewModel, PropertyInfo propertyInfo)
        {
            var rawGetter = (Func<TViewModel, TProperty>)propertyInfo.GetMethod.CreateDelegate(typeof(Func<TViewModel, TProperty>));
            return () => rawGetter(viewModel);
        }
    }
}