using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace TimeMeasurement_Backend.Entities.Constraints
{
    /// <summary>
    /// Attribute to check if string is an url
    /// </summary>
    public class IsUrl : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (Uri.TryCreate(value as string, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Input is not a valid URL");

        }
    }
}