using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeMeasurement_Backend.Entities
{
    public class Participant
    {
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public char Sex { get; set; }
        public string Nationality { get; set; }
        public int YearGroup { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string HouseNumber { get; set; }
        public string Team { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
