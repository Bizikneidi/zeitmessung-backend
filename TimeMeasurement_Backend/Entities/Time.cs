﻿namespace TimeMeasurement_Backend.Entities
{
    /// <summary>
    /// An Entity to record when somebody started and finished their run
    /// </summary>
    public class Time
    {
        /// <summary>
        /// The end time of the run
        /// </summary>
        public long End { get; set; }

        /// <summary>
        /// The start time of the run
        /// </summary>
        public long Start { get; set; }
    }
}