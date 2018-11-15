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
        MeasuredStop = 2 //Message contains a stop time
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
        //SERVER -> VIEWER
        Status = 0, //Message contains the current time measurement status
        RunStart = 1, //Message contains the time and all runners
        RunnerFinished = 2, //Message contains a runner who finished the race
        RunEnd = 4 //The run has ended (data is null)

        //VIEWER -> SERVER
        //nothing here yet...
    }

    public enum ParticipantCommands
    {
        //SERVER -> PARTICIPANT
        //nothing here yet...

        //PARTICIPANT -> SERVER
        Register = 0 //Message contains data to register as a participant
    }

}