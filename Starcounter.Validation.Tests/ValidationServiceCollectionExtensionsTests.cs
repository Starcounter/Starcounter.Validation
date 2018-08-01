using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Starcounter.Validation.Tests
{
    public class ValidationServiceCollectionExtensionsTests
    {
        [Test]
        public void ValidationBuilderCanBeCreatedWithoutValidationAttributeAdapter()
        {
            new ServiceCollection()

                .AddStarcounterValidation()

                .BuildServiceProvider()
                .GetService<IValidatorBuilder>()
                .Should().NotBeNull();
        }

        [Test]
        public void ValidationBuilderCanBeCreatedWithValidationAttributeAdapter()
        {
            new ServiceCollection()
                .AddSingleton(_ => Mock.Of<IValidationAttributeAdapter>())

                .AddStarcounterValidation()

                .BuildServiceProvider()
                .GetService<IValidatorBuilder>()
                .Should().NotBeNull();
        }
    }
}