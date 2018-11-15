using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeMeasurement_Backend.Entities;

namespace TimeMeasurement_Backend.Networking.Messaging
{
    /// <summary>
    /// entity to store the basic information of the current race
    /// Can be used by viewers and admin to calculate time differences and run local timer
    /// </summary>
    public class RunStartDTO
    {
        /// <summary>
        /// The current time of the station
        /// </summary>
        public long CurrentTime { get; set; }

        /// <summary>
        /// All the runners in a race
        /// </summary>
        public IEnumerable<Runner> Runners { get; set; }

        /// <summary>
        /// The start time of the station
        /// </summary>
        public long StartTime { get; set; }
    }

    /// <summary>
    /// entity to map time with starter number
    /// </summary>
    public class AssignmentDTO
    {
        /// <summary>
        /// The starter number of the runner
        /// </summary>
        public int Starter { get; set; }

        /// <summary>
        /// The time oof the runner
        /// </summary>
        public long Time { get; set; }
    }
}
