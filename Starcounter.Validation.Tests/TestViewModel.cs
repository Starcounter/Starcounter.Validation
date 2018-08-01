using System;
using System.ComponentModel.DataAnnotations;

namespace Starcounter.Validation.Tests
{
    public class TestViewModel
    {
        public const string FirstNameErrorMessage = nameof(FirstNameErrorMessage);
        public const string MaxLengthErrorMessage = nameof(MaxLengthErrorMessage);
        public const string EmailAddressErrorMessage = nameof(EmailAddressErrorMessage);

        [Required(ErrorMessage = FirstNameErrorMessage)]
        public string FirstName { get; set; }

        [MaxLength(10)]
        public string LastName { get; set; }

        [MaxLength(10, ErrorMessage = MaxLengthErrorMessage)]
        [EmailAddress(ErrorMessage = EmailAddressErrorMessage)]
        public string Email { get; set; }

        public string Password { get; set; }

        [Compare(nameof(Password))]
        public string RepeatPassword { get; set; }

        public string WriteOnly
        {
            set { throw new NotImplementedException(); }
        }

        protected string Protected { get; set; }

        public const string ProtectedPropertyName = nameof(Protected);

        public int Age { get; set; }
    }
}