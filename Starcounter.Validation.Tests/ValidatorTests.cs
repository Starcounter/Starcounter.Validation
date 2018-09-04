using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Starcounter.Validation.Tests
{
    public class ValidatorTests
    {
        private TestViewModel _viewModel;
        private IValidator _validator;
        private IDictionary<string, List<string>> _presentedErrors;
        private TestViewModel _subViewModel;
        private IDictionary<string, List<string>> _presentedSubValidatorErrors;
        private IValidator _subValidator;

        [SetUp]
        public void SetUp()
        {
            _viewModel = new TestViewModel();
            _subViewModel = new TestViewModel();
            _presentedErrors = new Dictionary<string, List<string>>();
            _presentedSubValidatorErrors = new Dictionary<string, List<string>>();
            _validator = SetupValidatorBuilder(new ValidatorBuilder())
                .Build();
        }

        [Test]
        public void ValidateThrowsIfPropertyHasNotBeenAddedBefore()
        {
            // exists on view-model, but never added to ValidatorBuilder
            var propertyName = nameof(TestViewModel.LastName);
            _validator.Invoking(validator => validator.Validate(propertyName, null))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(string.Format(Strings.Validator_PropertyNeverAdded, propertyName))
                ;
        }

        [Test]
        public void ValidateReturnsFalseAndAddsErrorsWhenValidationFails()
        {
            // FirstName is Required
            var propertyName = nameof(TestViewModel.FirstName);
            var validationResult = _validator.Validate(propertyName, null);

            using (new AssertionScope())
            {
                validationResult.Should().BeFalse();
                AssertErrorsContain(propertyName, TestViewModel.FirstNameErrorMessage);
            }
        }

        [Test]
        public void ValidateHandlesAttributesUsingValidationContext()
        {
            // FirstName is Required
            _viewModel.Password = "password";
            var propertyName = nameof(TestViewModel.RepeatPassword);
            var validationResult = _validator.Validate(propertyName, "repeat password");

            using (new AssertionScope())
            {
                validationResult.Should().BeFalse();
                AssertErrorsContain(propertyName, TestViewModel.RepeatPasswordErrorMessage);
            }
        }

        [Test]
        public void ValidateReturnsErrorsFromMultipleAttributes()
        {
            // Email is MaxLength(10) and EmailAddress
            var propertyName = nameof(TestViewModel.Email);
            var validationResult = _validator.Validate(propertyName, "aaa-aaa-aaa-aaa");

            using (new AssertionScope())
            {
                validationResult.Should().BeFalse();
                AssertErrorsContain(propertyName, TestViewModel.MaxLengthErrorMessage);
                AssertErrorsContain(propertyName, TestViewModel.EmailAddressErrorMessage);
            }
        }

        [Test]
        public void ValidateReturnsTrueAndClearsErrorsWhenValidationPassed()
        {
            // FirstName is Required
            var propertyName = nameof(TestViewModel.FirstName);
            var validationResult = _validator.Validate(propertyName, "John");

            using (new AssertionScope())
            {
                validationResult.Should().BeTrue();
                AssertErrorsAreCleared(propertyName);
            }
        }

        private void AssertErrorsAreCleared(string propertyName)
        {
            _presentedErrors.Should().ContainKey(propertyName)
                .WhichValue.Should().BeEmpty();
        }

        [Test]
        public void ValidateReturnsFalseWhenOneOfMultipleAttributesFails()
        {
            // Email is MaxLength(10) and EmailAddress
            var validationResult = _validator.Validate(nameof(TestViewModel.Email), "aaa");

            validationResult.Should().BeFalse();
        }

        [Test]
        public void ValidateAllReturnsFalseWhenAnyOfPropertyValidationFails()
        {
            // FirstName is required
            _viewModel.FirstName = null;

            _validator.ValidateAll().Should().BeFalse();
        }

        [Test]
        public void ValidateAllReturnsTrueAndClearsErrorsWhenAllOfPropertyValidationPasses()
        {
            // FirstName is required
            _viewModel.FirstName = "John";

            using (new AssertionScope())
            {
                _validator.ValidateAll().Should().BeTrue();
                AssertErrorsAreCleared(nameof(TestViewModel.FirstName));
                AssertErrorsAreCleared(nameof(TestViewModel.Email));
            }
        }

        [Test]
        public void ValidateAllPresentsAllErrors()
        {
            // FirstName is required
            _viewModel.FirstName = null;
            // Email is EmailAddress
            _viewModel.Email = "aaa";

            _validator.ValidateAll();

            using (new AssertionScope())
            {
                AssertErrorsContain(nameof(TestViewModel.FirstName), TestViewModel.FirstNameErrorMessage);
                AssertErrorsContain(nameof(TestViewModel.Email), TestViewModel.EmailAddressErrorMessage);
            }
        }

        [Test]
        public void ValidateAllPresentsAllErrorsFromSubValidators()
        {
            AddSubValidator();
            // FirstName is required
            _subViewModel.FirstName = null;

            _validator.ValidateAll();

            AssertSubValidatorErrorsContain(nameof(TestViewModel.FirstName), TestViewModel.FirstNameErrorMessage);
        }

        [Test]
        public void ValidateAllReturnsFalseWhenAnyOfPropertyValidationFromSubValidatorsFails()
        {
            AddSubValidator();
            // FirstName is required
            _viewModel.FirstName = "John";
            _subViewModel.FirstName = null;

            _validator.ValidateAll()
                .Should().BeFalse();
        }

        [Test]
        public void ValidateAllDoesntValidateDisposedSubValidator()
        {
            AddSubValidator();
            // FirstName is required
            _viewModel.FirstName = "John";
            _subViewModel.FirstName = null;

            _subValidator.Dispose();

            _validator.ValidateAll()
                .Should().BeTrue();
        }

        [Test]
        public void ValidateThrowsWhenValidatorIsDisposed()
        {
            _validator.Dispose();

            _validator.Invoking(v => v.Validate(nameof(TestViewModel.FirstName), null))
                .Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void ValidateAllThrowsWhenValidatorIsDisposed()
        {
            _validator.Dispose();

            _validator.Invoking(v => v.ValidateAll())
                .Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void CreateSubValidatorThrowsWhenValidatorIsDisposed()
        {
            _validator.Dispose();

            _validator.Invoking(v => v.CreateSubValidatorBuilder())
                .Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void ValidatorBuilderUsesValidationAttributeAdapterIfItsPresent()
        {
            // this actually test ValidatorBuilder, but since both classes are tightly coupled,
            // its easier to test it here. They could be decoupled, but that would require
            // a new interface, IValidatorFactory which would otherwise not make much sense
            var newErrorMessage = "error";
            var validator = SetupValidatorBuilder(new ValidatorBuilder(CreateAdapterReplacingErrorMessage(newErrorMessage)))
                .Build();
            var propertyName = nameof(TestViewModel.FirstName);

            validator.Validate(propertyName, null);

            AssertErrorsContain(propertyName, newErrorMessage);
        }

        private IValidationAttributeAdapter CreateAdapterReplacingErrorMessage(string newErrorMessage)
        {
            var adapterMock = new Mock<IValidationAttributeAdapter>();
            adapterMock.Setup(adapter => adapter.Adapt(It.IsAny<ValidationAttribute>(), It.IsAny<Type>()))
                .Returns((ValidationAttribute attribute, Type type) =>
                {
                    attribute.ErrorMessage = newErrorMessage;
                    return attribute;
                });

            return adapterMock.Object;
        }

        private void AssertErrorsContain(string propertyName, string error)
        {
            _presentedErrors.Should()
                .ContainKey(propertyName)
                .WhichValue.Should().Contain(error);
        }

        private void AssertSubValidatorErrorsContain(string propertyName, string error)
        {
            _presentedSubValidatorErrors.Should()
                .ContainKey(propertyName)
                .WhichValue.Should().Contain(error);
        }

        private IValidatorBuilder SetupValidatorBuilder(IValidatorBuilder builder)
        {
            return builder
                .WithViewModel(_viewModel)
                .WithResultsPresenter((name, errors) => _presentedErrors.Add(name, errors.ToList()))
                .AddProperty(nameof(TestViewModel.FirstName))
                .AddProperty(nameof(TestViewModel.RepeatPassword))
                .AddProperty(nameof(TestViewModel.Email));
        }

        private void AddSubValidator()
        {
            _subValidator = _validator.CreateSubValidatorBuilder()
                .WithViewModel(_subViewModel)
                .WithResultsPresenter((name, errors) => _presentedSubValidatorErrors.Add(name, errors.ToList()))
                .AddProperty(nameof(TestViewModel.FirstName))
                .Build();
        }
    }
}