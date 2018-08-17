using Starcounter.Uniform.FormItem;
using Starcounter.Uniform.Generic.FormItem;
using Starcounter.Uniform.ViewModels;

namespace Starcounter.Validation.Uniform
{
    public static class ValidatorBuilderFormItemExtensions
    {
        public static IValidator BuildWithFormItemMetadata(this IValidatorBuilder validatorBuilder,
            out FormItemMetadata formItemMetadata)
        {
            formItemMetadata = new FormItemMessagesBuilder()
                .ForProperties(validatorBuilder.Properties)
                .Build();
            // the reason this variable exists is that out vars can't be used inside a lambda
            var formItemMetadataLocal = formItemMetadata;
            validatorBuilder.WithResultsPresenter((name, errors) =>
                formItemMetadataLocal.SetMessage(name, string.Join(", ", errors), MessageType.Invalid));

            return validatorBuilder.Build();
        }
    }
}