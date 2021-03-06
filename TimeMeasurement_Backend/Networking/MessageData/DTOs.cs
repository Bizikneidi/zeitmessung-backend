﻿using System.Collections.Generic;
using TimeMeasurement_Backend.Entities;

namespace TimeMeasurement_Backend.Networking.MessageData
{
    /// <summary>
    /// Used to transfer basic data about the current race
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class RaceStartDTO
    {
        /// <summary>
        /// The current time of the station
        /// </summary>
        public long CurrentTime { get; set; }

        /// <summary>
        /// All the participants in the current race
        /// </summary>
        public IEnumerable<Participant> Participants { get; set; }

        /// <summary>
        /// The start time of the station
        /// </summary>
        public long StartTime { get; set; }
    }

    /// <summary>
    /// Used to transfer a starter number and a time to assign a participant his time
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class AssignmentDTO
    {
        /// <summary>
        /// The starter number of the participant
        /// </summary>
        public int Starter { get; set; }

        /// <summary>
        /// The time of the participant
        /// </summary>
        public long Time { get; set; }
    }
}