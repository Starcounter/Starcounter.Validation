﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Starcounter.Validation.Tests
{
    public class TestViewModel
    {
        public const string FirstNameErrorMessage = nameof(FirstNameErrorMessage);
        public const string MaxLengthErrorMessage = nameof(MaxLengthErrorMessage);
        public const string EmailAddressErrorMessage = nameof(EmailAddressErrorMessage);
        public const string RepeatPasswordErrorMessage = nameof(RepeatPasswordErrorMessage);

        [Required(ErrorMessage = FirstNameErrorMessage)]
        public string FirstName { get; set; }

        [MaxLength(10)]
        public string LastName { get; set; }

        [MaxLength(10, ErrorMessage = MaxLengthErrorMessage)]
        [EmailAddress(ErrorMessage = EmailAddressErrorMessage)]
        public string Email { get; set; }

        [CompareToProvidedString]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = RepeatPasswordErrorMessage)]
        public string RepeatPassword { get; set; }

        public string WriteOnly
        {
            set => throw new NotImplementedException();
        }

        protected string Protected { get; set; }

        public const string ProtectedPropertyName = nameof(Protected);

        [Range(0,10)]
        public int Age { get; set; }
    }
}