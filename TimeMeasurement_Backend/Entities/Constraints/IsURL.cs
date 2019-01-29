using System;
using System.ComponentModel.DataAnnotations;

namespace TimeMeasurement_Backend.Entities.Constraints
{
    /// <summary>
    /// Attribute to check if string is an url (RFC1738)
    /// </summary>
    public class IsUrl : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //URL can be Null
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