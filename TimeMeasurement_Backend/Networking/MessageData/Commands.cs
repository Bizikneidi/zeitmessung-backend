namespace TimeMeasurement_Backend.Networking.MessageData
{
    public enum StationCommands
    {
        //SERVER -> STATION
        StartMeasuring = 0, //Station should start measuring times (data is null)
        StopMeasuring = 1, //Station should stop measuring times (data is null)

        //STATION -> SERVER
        MeasuredStart = 2, //Station has started measuring and message contains the start time
        MeasuredStop = 3 //Station has measured a time and message contains a stop time
    }

    public enum AdminCommands
    {
        //SERVER -> ADMIN
        State = 0, //The state of the current race. Message contains the current race state
        RaceStart = 1, //A race has started. Message contains the start time and all runners
        MeasuredStop = 2, //A runner has finished. Message contains the corresponding stop time
        RaceEnd = 3, //The race has ended (data is null)

        //ADMIN -> SERVER
        Start = 4, //Admin has pressed the start button and server should start a race (data is null)
        AssignTime = 5 //Admin assigned a time to a runner. Message contains am AssignmentDTO
    }

    public enum ViewerCommands
    {
        //SERVER -> VIEWER
        State = 0, //The state of the current race. Message contains the current race state
        RaceStart = 1, //A race has started. Message contains the start time and all runners
        RunnerFinished = 2, //A runner has finished. Message contains a runner who finished the race
        RaceEnd = 4, //The race has ended (data is null)
        Races = 5, //Message contains all races up to this point
        Runners = 6, //Message contains all runners for a race

        //VIEWER -> SERVER
        GetRunners = 7 //Viewer is requesting all runners to a race. Message contains the race id
    }

    public enum ParticipantCommands
    {
        //SERVER -> PARTICIPANT
        //nothing here yet...

        //PARTICIPANT -> SERVER
        Register = 0 //A person wants to register. Message contains data to register as a participant
    }
}