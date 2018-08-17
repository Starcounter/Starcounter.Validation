using System;
using FluentAssertions;
using NUnit.Framework;

namespace Starcounter.Validation.Tests
{
    public class ValidatorBuilderTests
    {
        private ValidatorBuilder _builder;
        private TestViewModel _viewModel;
        private ValidationResultsPresenter _validationResultsPresenter;

        [SetUp]
        public void SetUp()
        {
            _builder = new ValidatorBuilder();
            _viewModel = new TestViewModel();
            _validationResultsPresenter = ValidationResultsPresenters.NullValidationResultsPresenter;
        }

        [Test]
        public void BuildsAValidatorIfEverythingIsSupplied()
        {
            _builder
                .WithViewModel(new object())
                .WithResultsPresenter(ValidationResultsPresenters.NullValidationResultsPresenter)
                .Build()
                .Should().NotBeNull();
        }

        [Test]
        public void PropertiesExposesAddedProperties()
        {
            PrepareBuilder()
                .AddProperty(nameof(TestViewModel.FirstName))
                .AddProperty(nameof(TestViewModel.LastName))
                .Properties.Should()
                .BeEquivalentTo(nameof(TestViewModel.FirstName), nameof(TestViewModel.LastName));
        }

        [Test]
        public void ThrowsIfViewModelHasNotBeenSetAndPropertiesAreAdded()
        {
            _builder
                .Invoking(builder => builder.AddProperty("Name"))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(string.Format(Strings.ValidatorBuilder_ViewModelMissing, nameof(IValidatorBuilder.WithViewModel)))
                ;
        }

        [Test]
        public void AllowsCreatingValidatorWithoutViewModel()
        {
            // such a thing could be used to group multiple sub-validators
            _builder
                .WithResultsPresenter(_validationResultsPresenter)
                .Build()
                .Invoking(validator => validator.ValidateAll())
                .Should().NotThrow();
        }

        [Test]
        public void ThrowsIfErrorPresenterHasNotBeenSet()
        {
            _builder
                .Invoking(builder => builder.WithViewModel(_viewModel).Build())
                .Should().Throw<InvalidOperationException>()
                .WithMessage(string.Format(Strings.ValidatorBuilder_ResultsPresenterMissing, nameof(IValidatorBuilder.WithResultsPresenter)))
                ;
        }

        [Test]
        public void ThrowsIfNonExistentPropertyIsAdded()
        {
            var propertyName = "unknown";
            InvokingOnValidBuilder(builder => builder.AddProperty(propertyName))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(String.Format(Strings.ValidatorBuilder_ViewModelMissingProperty,
                    typeof(TestViewModel).FullName, propertyName));
        }

        [Test]
        public void ThrowsIfProtectedPropertyIsAdded()
        {
            var propertyName = TestViewModel.ProtectedPropertyName;

            InvokingOnValidBuilder(builder => builder.AddProperty(propertyName))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(String.Format(Strings.ValidatorBuilder_ViewModelMissingProperty,
                    typeof(TestViewModel).FullName, propertyName));
        }

        [Test]
        public void ThrowsIfWriteOnlyPropertyIsAdded()
        {
            var propertyName = nameof(TestViewModel.WriteOnly);
            InvokingOnValidBuilder(builder => builder.AddProperty(propertyName))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(string.Format(Strings.ValidatorBuilder_PropertyGetterMissing, typeof(TestViewModel).FullName, propertyName));
        }

        [Test]
        public void ThrowsIfPropertyIsAddedAgain()
        {
            var propertyName = nameof(TestViewModel.FirstName);
            InvokingOnValidBuilder(builder => builder
                    .AddProperty(propertyName)
                    .AddProperty(propertyName
                    ))
                .Should().Throw<InvalidOperationException>()
                .WithMessage(string.Format(Strings.ValidatorBuilder_PropertyAlreadyAdded, propertyName))
                ;
        }

        private IValidatorBuilder PrepareBuilder()
        {
            return _builder
                .WithResultsPresenter(_validationResultsPresenter)
                .WithViewModel(_viewModel);
        }

        private Action InvokingOnValidBuilder(Action<IValidatorBuilder> action)
        {
            return PrepareBuilder().Invoking(action);
        }
    }
}