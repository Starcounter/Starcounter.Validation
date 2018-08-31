# Starcounter.Validation

This library helps validating Starcounter view-models annotated with `ValidationAttributes`, defined in `System.ComponentModel.DataAnnotations` and custom.

## Where to find it?

Install this library from NuGet:

```
Install-Package Starcounter.Validation
```

## Creating `IValidatorBuilder`

The entry point to this library is `IValidatorBuilder` interface and its implementation, `ValidationBuilder`. 
If you're using Dependency Injection with [Starcounter.Startup](https://github.com/Starcounter/Starcounter.Startup), simply add this feature in your `Startup` class:

```c#
public class Startup: IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ...
        services.AddStarcounterValidation();
    }
    
    // ...
}
```

If you're not using DI, you can construct `ValidationBuilder` by hand:

```c#
var validationBuilder = new ValidationBuilder();
```

## Declaring validation rules in the view-model

This library makes use of [`ValidationAttribute`](https://docs.microsoft.com/en-gb/dotnet/api/system.componentmodel.dataannotations.validationattribute?view=netframework-4.7.1). To use it, you have to add a reference to `System.ComponentModel.DataAnnotations` assembly. You can use attributes provided by Microsoft in [System.ComponentModel.DataAnnotations namespace](https://docs.microsoft.com/en-gb/dotnet/api/system.componentmodel.dataannotations?view=netframework-4.7.1) or create your own custom attributes. For details on how to create and use them, refer to [docs.microsoft.com](https://docs.microsoft.com).

This readme contains a small example of how you can annotate your view-model with validation attributes. Consider the following view-model:

```json
{
    "Name$": "",
    "Breed": "",
    "ValidationErrors": ""
}
```

To annotate it, you have to repeat its properties in the code-behind:

```c#
public partial class DogViewModel: Json
{
    [Required]
    public string Name {get; set;}

    [MaxLength(120)]
    public string Breed {get; set;}
}
```

Here, the `Name` is required not to be empty, and `Breed` can't be longer than 120 characters.
To validate this view-model, first obtain an instance of `IValidatorBuilder`. If you're using DI, declare it in your `Init`. Otherwise create it manually.
Next, you have to call `WithViewModel` to specify the view-model instance you want to validate, `WithResultsPresenter` to specify how to present the validation errors and call `AddProperty` for each property you want to validate.

```c#
public partial class DogViewModel: Json, IInitPageWithDependencies
{
    private IValidator _validator;

    public void Init(IValidatorBuilder validationBuilder)
    {
        _validator = validatorBuilder
            .WithViewModel(this)
            .AddProperty(nameof(Name))
            .AddProperty(nameof(Breed))
            .WithResultsPresenter(PresentErrors)
            .Build();
    }

    private void PresentErrors(string propertyName, IEnumerable<string> errors)
    {
        ValidationErrors +=
            $"Property {propertyName} failed validation with following errors: {string.Join(", ", errors)}";
    }
}
```

### ValidatorBuilder extension methods

There are two extension methods defined in ValidatorBuilderExtensions:

```c#
public static IValidatorBuilder AddProperties(this IValidatorBuilder validatorBuilder, params string[] properties)
public static IValidatorBuilder WithViewModelAndAllProperties(this IValidatorBuilder validatorBuilder, object viewModel)
```

`AddProperties` lets you add a number of properties at once:

```c#
// instead of this:
validatorBuilder
    .WithViewModel(this)
    .AddProperty(nameof(Name))
    .AddProperty(nameof(Breed));
// you can write this:
validatorBuilder
    .AddProperties(nameof(Name), nameof(Breed));
```

`WithViewModelAndAllProperties` lets you set the view-model and all its public properties (but only those with at least one `ValidationAttribute` applied) in one go:

```c#
// instead of this:
validatorBuilder
    .WithViewModel(this)
    .AddProperty(nameof(Name))
    .AddProperty(nameof(Breed));
// you can write this:
validatorBuilder
    .WithViewModelAndAllProperties(this);
// (under the assumption that Name and Breed are all the public, validated properties)
```






## Using IValidator

The `IValidator` interface exposes two methods for validation:

### Validate

```c#
bool Validate(string propertyName, object value);
```

Validates the supplied value, using rules from the specified property. Returns true if validation succeeded.
The most common use of this would be in an input handler:

```c#
public void Handle(Input.Name input)
{
    // this will use ResultsPresenter to display or clear validation errors for Name
    _validator.Validate(nameof(Name), input.Value);
}
```

You can also use the return value to stop further processing if validation fails:

```c#
public void Handle(Input.Name input)
{
    // this will use ResultsPresenter to display or clear validation errors for Name
    if(!_validator.Validate(nameof(Name), input.Value))
    {
        return;
    }

    // further processing
}
```

### ValidateAll

```c#
bool ValidateAll();
```

Validates current values of all configured properties. Also calls `ValidateAll` on all the sub validators (see "Validating collections").
Returns true if all the validation succeeded, false if any of it failed. Note that, unlike Validate, it uses actual values of the properties.

This method is usually called before saving the changes or performing a major action to make sure that the view-model is valid as a whole.

```c#
public void Handle(Input.SaveTrigger input)
{
    // this will use ResultsPresenter to display or clear validation errors for Name
    if(!_validator.ValidateAll())
    {
        return;
    }

    AttachedScope.Commit();
}
```

## Integrating with Starcounter.Uniform

Most commonly, you would not write your custom error presenter, but rather use `<uni-form-item>`, a custom element provided by Uniform for cases like this.
To use it, add `Starcounter.Validation.Uniform` as a reference to your project:

```
Install-Package Starcounter.Validation.Uniform
```

Next, add it to your view-model:

```js
{
    "Name$": "",
    "Breed": "",
    "ValidationErrors": "",

    "FormItemMetadata": {}
}
```

And finally, use `BuildWithFormItemMetadata` instead of `WithResultsPresenter` and `Build`:

```c#
public void Init(IValidatorBuilder validationBuilder)
{
    _validator = validatorBuilder
        .WithViewModel(this)
        .AddProperty(nameof(Name))
        .AddProperty(nameof(Breed))
        .BuildWithFormItemMetadata(out var formItemMetadata);
    FormItemMetadata = formItemMetadata;
}
```

Note that you don't have to specify `DefaultTemplate.FormItemMetadata.InstanceType`.

## Validating collections

Suppose you have a view-model, where you allow editing elements of a collection and you want to validate it. To do that, you have to add a validator to every element of the collection.
In this example, we will use `<uni-form-item>` integration described in the previous paragraph.

```js
{
    "PackName$": "",
    "FormItemMetadata": {},
    "PackMembers": [{
        "Name$": ""
        "FormItemMetadata": {}
    }]
}
```

To create validators for the collection, use `CreateSubValidatorBuilder` method:

```c#
public class Dog
{
    public string Name {get; set;}
    public string Breed {get; set;}
}

public class Pack
{
    public string PackName {get; set;}

    public IEnumerable<Dog> GetMembers()
    {
        // ...
    }
}

public partial class PackViewModel: Json, IBound<Pack>, IInitWithDependencies
{
    public void Init(IValidatorBuilder validatorBuilder)
    {
        _validator = validatorBuilder
            .WithViewModel(this)
            .AddProperty(nameof(PackName))
            .BuildWithFormItemMetadata(out var formItemMetadata);
        FormItemMetadata = formItemMetadata;
    
        foreach(var dog in Data.GetMembers())
        {
            PackMembers.Add().Init(dog, _validator.CreateSubValidatorBuilder())
        }
    }

    [DogViewModel_json.PackMembers]
    public partial class PackMemberViewModel : Json, IBound<Dog>
    {
        private IValidator _validator;

        [Required]
        public string Name { get; set; }

        public void Init(Dog dog, IValidatorBuilder validatorBuilder)
        {
            Data = dog;

            _validator = validatorBuilder
                .WithViewModel(this)
                .AddProperty(nameof(Name))
                .BuildWithFormItemMetadata(out var formItemMetadata);
            // it refers here to FormItemMetadata of PackMemberViewModel, not of PackViewModel
            FormItemMetadata = formItemMetadata;
        }
    }
}

```

