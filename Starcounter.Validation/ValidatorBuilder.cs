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
        /// TODO
        /// </summary>
        /// <param name="validator"></param>
        public delegate void ValidatorBuildHandler(IValidator validator);

        private static readonly MethodInfo CreateGetterMethodInfo =
            typeof(ValidatorBuilder)
                .GetMethod(nameof(CreateGetter), BindingFlags.NonPublic | BindingFlags.Static);

        private readonly IValidationAttributeAdapter _validationAttributeAdapter;
        private readonly ValidatorBuildHandler _validatorBuildHandler;
        private readonly IDictionary<string, Validator.PropertyValidationData> _properties = new Dictionary<string, Validator.PropertyValidationData>();

        private object _viewModel;
        private ErrorPresenter _errorPresenter;

        /// <summary>
        /// Constructs a new instance of <see cref="ValidatorBuilder"/>.
        /// </summary>
        /// <param name="validationAttributeAdapter">Used when scanning properties for <see cref="ValidationAttribute"/>. If this is null, no adaptation will occur.</param>
        public ValidatorBuilder(IValidationAttributeAdapter validationAttributeAdapter = null) : this(validationAttributeAdapter, null)
        {
        }

        /// <summary>
        /// Constructs a new instance of <see cref="ValidatorBuilder"/>. This constructor is meant to be used by <see cref="Validator.AddSubValidator"/>
        /// </summary>
        /// <param name="validationAttributeAdapter">Used when scanning properties for <see cref="ValidationAttribute"/>. If this is null, no adaptation will occur.</param>
        /// <param name="validatorBuildHandler">Called after <see cref="Build"/> is created.</param>
        public ValidatorBuilder(IValidationAttributeAdapter validationAttributeAdapter, ValidatorBuildHandler validatorBuildHandler)
        {
            _validationAttributeAdapter = validationAttributeAdapter;
            _validatorBuildHandler = validatorBuildHandler;
        }

        public IValidatorBuilder WithViewModel(object viewModel)
        {
            _viewModel = viewModel;
            return this;
        }

        public IValidatorBuilder AddProperty(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (_properties.ContainsKey(propertyName))
            {
                throw new InvalidOperationException(
                    string.Format(Strings.ValidatorBuilder_PropertyAlreadyAdded, propertyName));
            }

            var viewModelType = _viewModel.GetType();
            var property = viewModelType.GetProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(string.Format(Strings.ValidatorBuilder_ViewModelMissingProperty, _viewModel, propertyName));
            }

            if (!property.CanRead)
            {
                throw new InvalidOperationException(string.Format(Strings.ValidatorBuilder_PropertyGetterMissing, viewModelType, propertyName));
            }

            _properties[propertyName] = new Validator.PropertyValidationData()
            {
                Attributes = GetValidationAttributesFromProperty(property),
                Getter = (Func<object>)CreateGetterMethodInfo
                        // CreateGetter has no generic constraints, so this should never fail
                    .MakeGenericMethod(viewModelType, property.PropertyType)
                    .Invoke(null, new[] { _viewModel, property })
            };

            return this;
        }

        private List<ValidationAttribute> GetValidationAttributesFromProperty(PropertyInfo property)
        {
            var attributes = property.GetCustomAttributes<ValidationAttribute>();
            if (_validationAttributeAdapter != null)
            {
                attributes = attributes.Select(_validationAttributeAdapter.Adapt);
            }
            return attributes.ToList();
        }

        public IValidatorBuilder WithErrorPresenter(ErrorPresenter errorPresenter)
        {
            _errorPresenter = errorPresenter;
            return this;
        }

        public IEnumerable<string> Properties => _properties.Keys;

        public IValidator Build()
        {
            if (_errorPresenter == null)
            {
                throw new InvalidOperationException(string.Format(Strings.ValidatorBuilder_ErrorPresenterMissing, nameof(WithErrorPresenter)));
            }
            if (_viewModel == null)
            {
                throw new InvalidOperationException(string.Format(Strings.ValidatorBuilder_ViewModelMissing, nameof(WithViewModel)));
            }

            var validator = new Validator(_errorPresenter, _properties, _viewModel, CloneWithBuildHandler);
            _validatorBuildHandler?.Invoke(validator);
            return validator;
        }

        private IValidatorBuilder CloneWithBuildHandler(ValidatorBuildHandler buildHandler)
        {
            return new ValidatorBuilder(_validationAttributeAdapter, buildHandler);
        }

        private static Func<TProperty> CreateGetter<TViewModel, TProperty>(TViewModel viewModel,
            PropertyInfo propertyInfo)
        {
            var rawGetter = (Func<TViewModel, TProperty>)propertyInfo.GetMethod.CreateDelegate(typeof(Func<TViewModel, TProperty>));
            return () => rawGetter(viewModel);
        }
    }
}