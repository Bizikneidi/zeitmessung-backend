using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeMeasurement_Backend.Entities
{
    public class Participant
    {
        /// <summary>
        /// Lastname
        /// Needs to be a single word or more words seperated with a '-'
        /// Capital initial letter, rest lower case
        /// minimum 2 letters
        /// required
        /// </summary>
        [Required]
        [RegularExpression(@"^[A-ZÄÜÖ][a-zäüö]+(-[A-ZÄÜÖ][a-zäüö]+)?$")]
        public string Lastname { get; set; }

        /// <summary>
        /// Multiple words separated by spaces
        /// Capital initial letter, rest lower case
        /// Within words only letters
        /// At least two letters per name
        /// required
        /// </summary>
        [Required]
        [RegularExpression(@"^([A-ZÄÜÖ][a-zäüö]+)([ ][A-ZÄÜÖ][a-zäüö]+)*$")]
        public string Firstname { get; set; }

        /// <summary>
        /// only m w or s
        /// required
        /// </summary>
        [Required]
        [RegularExpression(@"^[mws]$")]
        public char Sex { get; set; }

        public string Nationality { get; set; }

        /// <summary>
        /// first year group: 1920
        /// </summary>
        [Required]
        [Range(1919, 3000)]
        public int YearGroup { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z][a-z0-9-:/]*$")]
        public string PostalCode { get; set; }

        [Required]
        [RegularExpression(@"^([A-Za-z ./-])*$")]
        public string City { get; set; }

        [Required]
        [RegularExpression(@"^([A - Za - z./ -])*$")]
        public string Street { get; set; }

        [Required]
        [RegularExpression(@"^([0-9a-zA-Z/])*$")]
        public string HouseNumber { get; set; }

        public string Team { get; set; }

        [Required]
        //[RegularExpression("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\x01 -\x08\x0b\x0c\x0e -\x1f\x21\x23 -\x5b\x5d -\x7f] |\[\x01-\x09\x0b\x0c\x0e-\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])")]
        public string Email { get; set; }

        [RegularExpression("^(?:(?:\\(?(?:00|\\+)([1-4]\\d\\d|[1-9]\\d?)\\)?)?[\\-\\.\\ \\\\\\/]?)?((?:\\(?\\d{1,}\\)?[\\-\\.\\ \\\\\\/]?){0,})(?:[\\-\\.\\ \\\\\\/]?(?:#|ext\\.?|extension|x)[\\-\\.\\ \\\\\\/]?(\\d+))?$")]
        public string PhoneNumber { get; set; }
    }
}
