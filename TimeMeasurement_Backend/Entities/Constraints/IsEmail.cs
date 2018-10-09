using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace TimeMeasurement_Backend.Entities.Constraints
{
    /// <summary>
    /// Attribute to check if string is an email
    /// </summary>
    public class IsEmail : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //Email must not be null
            if (value == null)
            {
                return new ValidationResult("Input was null");
            }

            try
            {
                //Try to convert string to mailaddress
                var mailAddress = new MailAddress((string)value);
                return ValidationResult.Success;
            }
            catch (Exception ex)
            {
                //Exeception, when conversion is not possible
                return new ValidationResult(ex.Message);
            }
        }
    }
}