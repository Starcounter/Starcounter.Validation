# Starcounter.Validation

This library helps validating Starcounter view-models annotated with `ValidationAttributes`, defined in `System.ComponentModel.DataAnnotations` and custom.

## Where to find it?

Install this library from NuGet:

```
Install-Package Starcounter.Validation
```

## Creating `IValidationBuilder`

The entry point to this library is `IValidationBuilder` interface and its implementation, `ValidationBuilder`. 
If you're using Dependency Injection with [Starcounter.Startup](https://github.com/Starcounter/Starcounter.Startup), simply add this feature in your `Startup` class:

```c#
public class Startup: IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ...
        services.AddValidation();
    }
    
    // ...
}
```

If you're not using DI, you can just construct `ValidationBuilder` by hand:

```c#
var validationBuilder = new ValidationBuilder();
```

## Declaring validation rules in the view-model

This library makes use of [`ValidationAttribute`](https://docs.microsoft.com/en-gb/dotnet/api/system.componentmodel.dataannotations.validationattribute?view=netframework-4.7.1). You can use attributes provided by Microsoft in [System.ComponentModel.DataAnnotations namespace](https://docs.microsoft.com/en-gb/dotnet/api/system.componentmodel.dataannotations?view=netframework-4.7.1) or create your own custom attributes. For details on how to create and use them, refer to docs.microsoft.com.

This readme contains a small example of how you can annotate your view-model with validation attributes. Consider the following view-model:

```json
{
    "Name$": "",
    "Description$": "",
    "ValidationErrors": ""
}
```

To annotate it, you have to repeat its properties in the code-behind:

```c#
public partial class MyViewModel: Json
{
    [Required]
    public string Name {get; set;}

    [MaxLength(120)]
    public string Description {get; set;}
}
```

Here, the `Name` is required not to be empty, and `Description` can't be longer than 120 characters.
To validate this view-model, first obtain an instance of `IValidatorBuilder`. If you're using DI, just declare it in your `Init`. Otherwise just create it manually.
Next, you have to call `WithViewModel` to specify the view-model instance you want to validate, `WithErrorPresenter` to specify how to present the validation errors and call `AddProperty` for each property you want to validate.

```c#
public partial class MyViewModel: Json, IInitPageWithDependencies
{
    private IValidator _validator;

    public void Init(IValidationBuilder validationBuilder)
    {
        _validator = validatorBuilder
            .WithViewModel(this)
            .AddProperty(nameof(Name))
            .WithErrorPresenter(PresentErrors)
    }

    private void PresentErrors(string propertyName, IEnumerable<string> errors)
    {
        ValidationErrors +=
            $"Property {propertyName} failed validation with following errors: {string.Join(", ", errors)}";
    }
}
```

Your validator is complete, you can now call its `ValidateAll()` method

## Sub-Validators

## Integrating with Starcounter.Uniform

