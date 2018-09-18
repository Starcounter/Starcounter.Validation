using FluentAssertions;
using NUnit.Framework;

namespace Starcounter.Validation.Tests
{
    public class ValidatorBuilderExtensionsTests
    {
        [Test]
        public void WithViewModelAndAllProperties_AddsAllTheProperties()
        {
            var viewModel = new TestViewModel();

            new ValidatorBuilder()
                .WithViewModelAndAllProperties(viewModel)
                .Properties
                .Should().BeEquivalentTo(
                    nameof(TestViewModel.FirstName),
                    nameof(TestViewModel.LastName),
                    nameof(TestViewModel.Email),
                    nameof(TestViewModel.RepeatPassword),
                    nameof(TestViewModel.Age));
            // if the view-model weren't set, ValidatorBuilder would throw exception at AddProperty
            // there is no easy way to check what view-model was set
        }
    }
}