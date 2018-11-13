using System.Collections.Generic;
using TimeMeasurement_Backend.Entities.Constraints;

namespace TimeMeasurement_Backend.Entities
{
    public class Race
    {
        public int Id { get; set; }
        public long Date { get; set; }
        public ICollection<ParticipantTime> Timetable { get; set; }
    }
}