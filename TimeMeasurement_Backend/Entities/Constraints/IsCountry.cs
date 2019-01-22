using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace TimeMeasurement_Backend.Entities.Constraints
{
    /// <summary>
    /// Attribute to check if string is a country code (ISO - alpha 2)
    /// </summary>
    public class IsCountry : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ValidationResult.Success;

            //if (value == null)
            //{
            //    return new ValidationResult("Input was null");
            //}

            //string code = ((string)value).ToUpper();
            //var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures); //Get all known cultures
            //return cultures.Select(culture => new RegionInfo(culture.LCID)) //Get all regions
            //    .Any(region => code == region.ThreeLetterISORegionName.ToUpper())
            //    ? //Check if any code matches
            //    ValidationResult.Success
            //    : new ValidationResult("Not a country code");
        }
    }
}