using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Chinese_sale_api.Validations
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        //isvalid
        protected override ValidationResult IsValid(object? value,ValidationContext validationContex)
        {
            var password = value as string;
            if (string.IsNullOrEmpty(password) || password.Length < 7 || password.Length >15)
            {
                return new ValidationResult("password must contain 7-15 characters");
            }
            if (!password.Any(char.IsLower))
            {
                return new ValidationResult("The password must contain at least one lowercase letter.");
            }

            if (!password.Any(char.IsUpper))
            {
                return new ValidationResult("The password must contain at least one uppercase letter.");
            }

            if (!password.Any(char.IsDigit))
            {
                return new ValidationResult("The password must contain at least one digit.");
            }

            return ValidationResult.Success;
        }
    }
}
