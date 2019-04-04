# Starcounter.Validation

This library helps validating Starcounter view-models annotated with `ValidationAttributes`, defined in `System.ComponentModel.DataAnnotations` and custom.

## Table of contents

- [Installation](#installation)
- [Creating `IValidatorBuilder`](#creating-ivalidatorbuilder)
- [Declaring validation rules in the view-model](#declaring-validation-rules-in-the-view-model)
  * [Specifying a subset of properties](#specifying-a-subset-of-properties)
- [Using IValidator](#using-ivalidator)
  * [Validate](#validate)
  * [ValidateAll](#validateall)
- [Integrating with Starcounter.Uniform](#integrating-with-starcounteruniform)
- [Custom validation rules](#custom-validation-rules)
- [Validating using services](#validating-using-services)
- [Validating collections](#validating-collections)

<small><i><a href='http://ecotrust-canada.github.io/markdown-toc/'>Table of contents generated with markdown-toc</a></i></small>

## Installation

[This package is available on nuget](https://www.nuget.org/packages/Starcounter.Validation/). You can get it there. To install with CLI run:

```
Install-Package Starcounter.Validation
```

## Requirements
Requires Starcounter 2.4.0.7243 or later and .NET Framework 4.6.2.

## Creating `IValidatorBuilder`

The entry point to this library is `IValidatorBuilder` interface and its implementation, `ValidatorBuilder`. 
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

If you're not using DI, you can construct `ValidatorBuilder` by hand:

```c#
var validatorBuilder = new ValidatorBuilder();
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
public partial class DogViewModel: Json, IBound<Dog>
{
    [Required]
    public string Name { get => Data.Name; set => Data.Name = value; }

    [MaxLength(120)]
    public string Breed { get => Data.Breed; set => Data.Breed = value; }
}
```

⚠️ If you specify your properties only in json, you don't need to do anything to have them bound to `Data`. However, if also declare them in your code-behind, you have to explicitly bind them, as shown in the example above.

Here, the `Name` is required not to be empty, and `Breed` can't be longer than 120 characters.
To validate this view-model, first obtain an instance of `IValidatorBuilder`. If you're using DI, declare it in your `Init`. Otherwise create it manually.
Next, you can call `WithViewModelAndAllProperties` to specify the view-model instance you want to validate and use all of its validatable properties, `WithResultsPresenter` to specify how to present the validation errors.

```c#
public partial class DogViewModel: Json, IBound<Dog>, IInitPageWithDependencies
{
    private IValidator _validator;

    public void Init(IValidatorBuilder validatorBuilder)
    {
        _validator = validatorBuilder
            .WithViewModelAndAllProperties(this)
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

### Specifying a subset of properties

Instead of calling `WithViewModelAndAllProperties(this)` you can specify view-model and properties manually:

```c#
        _validator = validatorBuilder
            .WithViewModel(this)
            .AddProperty(nameof(Name)) // or use ValidatorBuilderExtensions.AddProperties
            .AddProperty(nameof(Breed))
            .WithResultsPresenter(PresentErrors)
            .Build();
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
public void Init(IValidatorBuilder validatorBuilder)
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

## Custom validation rules

When possible, try to reuse existing validation attributes from the [System.ComponentModel.DataAnnotations namespace](https://docs.microsoft.com/en-gb/dotnet/api/system.componentmodel.dataannotations?view=netframework-4.7.1). However, when it's not possible, you can create your own custom validation attributes. When doing this, respect following guidelines:

1. Your custom attribute class must derive from `ValidationAttribute` and its name should end with `Attribute`.
2. You can either override `bool IsValid(object)` or `ValidationResult IsValid(object, ValidationContext)`.
2. If the value you're validiting is null (or empty), return validation success. If your data shouldn't be null nor empty, apply `[Required]` to your properties.
3. When returning validation errors you should return `new ValidationResult(FormatErrorMessage(validationContext.DisplayName))`. That way future users of your attribute can customize the error message with `ErrorMessage`, `ErrorMessageResourceName` and `ErrorMessageResourceType`.

## Validating using services

When you want to use services in your custom validation attributes - including database access services like a repository - use `validationContext`, as shown below:

```c#
public class UniqueUsernameAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null) // always return success on null, it's a job for [Required]
        {
            return ValidationResult.Success;
        }
        var usersRepository = validationContext.GetRequiredService<IUsersRepository>();
        return usersRepository.GetUserByName((string)value) == null
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }

    public override bool RequiresValidationContext => true;
}
```


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

public partial class PackViewModel : Json, IBound<Pack>
{
    private IValidator _validator;

    public void Init(IValidatorBuilder validatorBuilder)
    {
        _validator = validatorBuilder
            .WithViewModelAndAllProperties(this)
            .BuildWithFormItemMetadata(out var formItemMetadata);
        FormItemMetadata = formItemMetadata;

        foreach (var dog in Data.GetMembers())
        {
            AddMemberViewModel(dog);
        }
    }

    public void Handle(Input.AddTrigger trigger)
    {
        AddMemberViewModel(new Dog());
    }

    private void AddMemberViewModel(Dog dog)
    {
        var packMemberViewModel = new PackMemberViewModel();
        packMemberViewModel.Init(dog, _validator.CreateSubValidatorBuilder());
        PackMembers.Add(packMemberViewModel);
    }

    [PackViewModel_json.PackMembers]
    public partial class PackMemberViewModel : Json, IBound<Dog>
    {
        private IValidator _validator;

        [Required]
        public string Name
        {
            get => Data.Name;
            set => Data.Name = value;
        }

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

If you allow removing elements from the list, you will also need to dispose of their validators when you do that:

```c#
public partial class PackViewModel : Json, IBound<Pack>
{
    // some members omitted for brevity
    
    private IValidator _validator;

    private void AddMemberViewModel(Dog dog)
    {
        var packMemberViewModel = new PackMemberViewModel();
        // pass RemoveMember
        packMemberViewModel.Init(dog, RemoveMember, _validator.CreateSubValidatorBuilder());
        PackMembers.Add(packMemberViewModel);
    }

    private void RemoveMember(PackMemberViewModel packMemberViewModel)
    {
        packMemberViewModel.Data.Delete();
        PackMembers.Remove(packMemberViewModel);
        // call view-model's Dispose method, which in turn disposes of the validator
        packMemberViewModel.Dispose();
    }

    [PackViewModel_json.PackMembers]
    public partial class PackMemberViewModel : Json, IBound<Dog>, IDisposable
    {
        private IValidator _validator;
        private Action<PackMemberViewModel> _removeAction;

        public void Init(Dog dog, Action<PackMemberViewModel> removeAction, IValidatorBuilder validatorBuilder)
        {
            _removeAction = removeAction;
        }

        public void Dispose()
        {
            // Dispose will detach this sub-validator from its parent
            // and it will no longer respond to its parent ValidateAll
            _validator?.Dispose();
        }
    }
}
```

The full example is presented below:

```c#
public partial class PackViewModel : Json, IBound<Pack>
{
    private IValidator _validator;

    public void Init(IValidatorBuilder validatorBuilder)
    {
        _validator = validatorBuilder
            .WithViewModelAndAllProperties(this)
            .BuildWithFormItemMetadata(out var formItemMetadata);
        FormItemMetadata = formItemMetadata;

        foreach (var dog in Data.GetMembers())
        {
            AddMemberViewModel(dog);
        }
    }

    public void Handle(Input.AddTrigger trigger)
    {
        AddMemberViewModel(new Dog());
    }

    private void AddMemberViewModel(Dog dog)
    {
        var packMemberViewModel = new PackMemberViewModel();
        packMemberViewModel.Init(dog, RemoveMember, _validator.CreateSubValidatorBuilder());
        PackMembers.Add(packMemberViewModel);
    }

    private void RemoveMember(PackMemberViewModel packMemberViewModel)
    {
        packMemberViewModel.Data.Delete();
        PackMembers.Remove(packMemberViewModel);
        packMemberViewModel.Dispose();
    }

    [PackViewModel_json.PackMembers]
    public partial class PackMemberViewModel : Json, IBound<Dog>, IDisposable
    {
        private IValidator _validator;
        private Action<PackMemberViewModel> _removeAction;

        [Required]
        public string Name
        {
            get => Data.Name;
            set => Data.Name = value;
        }

        public void Init(Dog dog, Action<PackMemberViewModel> removeAction, IValidatorBuilder validatorBuilder)
        {
            Data = dog;
            _removeAction = removeAction;

            _validator = validatorBuilder
                .WithViewModel(this)
                .AddProperty(nameof(Name))
                .BuildWithFormItemMetadata(out var formItemMetadata);
            // it refers here to FormItemMetadata of PackMemberViewModel, not of PackViewModel
            FormItemMetadata = formItemMetadata;
        }

        public void Handle(Input.RemoveTrigger trigger)
        {
            _removeAction(this);
        }

        public void Dispose()
        {
            _validator?.Dispose();
        }
    }

}
```
