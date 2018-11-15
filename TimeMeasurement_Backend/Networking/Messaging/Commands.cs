using System.Collections.Generic;
using TimeMeasurement_Backend.Entities;

namespace TimeMeasurement_Backend.Networking.Messaging
{
    public enum StationCommands
    {
        //SERVER -> STATION
        StartMeasuring = 0, //Station should start measuring the time

        //STATION -> SERVER
        MeasuredStart = 1, //Message contains the start time
        MeasuredStop = 2 //Message contains the stop time
    }

    public enum AdminCommands
    {
        //SERVER -> ADMIN
        Status = 0, //Message contains the current time measurement status
        RunStart = 1, //Message contains the time and all runners
        MeasuredStop = 2, //Message contains a stop time

        //ADMIN -> SERVER
        Start = 4, //Admin has pressed the start button and server should start a race
        AssignTime = 5 //Admin assigned a time to a runner
    }

    public enum ViewerCommands
    {
        Status = 0, //Message contains the current time measurement status
        RunStart = 1, //Message contains the time and all runners
        MeasuredStop = 2, //Message contains a stop time
        RunEnd = 4 //The run has ended (data is null)
    }

    public enum ParticipantCommands
    {
        Register = 0 //Message contains data to register as a participant
    }

    /// <summary>
    /// entity to store the start and current time for run
    /// Can be used by viewers and admin to calculate time differences and run local timer
    /// </summary>
    public class RunStart
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
}