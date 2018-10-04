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
        public string PostalCode { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string HouseNumber { get; set; }
        public string Team { get; set; }
        [Required]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
