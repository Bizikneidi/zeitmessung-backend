namespace TimeMeasurement_Backend.Networking.MessageData
{
    public enum StationCommands
    {
        //SERVER -> STATION
        StartMeasuring = 0, //Station should start measuring times, data is null
        StopMeasuring = 1, //Station should stop measuring times, data is null

        //STATION -> SERVER
        MeasuredStart = 2, //Station has started measuring, data is the start time
        MeasuredStop = 3 //Station has measured a time, data is a stop time
    }

    public enum AdminCommands
    {
        //SERVER -> ADMIN
        State = 0, //The state of the current race, data is the state
        AvailableRaces = 1, //All available races the admin can start, data is a list of races
        RaceStart = 2, //A race has started, data is a RaceStartDTO
        MeasuredStop = 3, //A participant has finished, data is the corresponding stop time
        RaceEnd = 4, //The race has ended, data is null

        //ADMIN -> SERVER
        Start = 5, //Admin has pressed the start button and server should start a race. data is the id of a race
        AssignTime = 6, //Admin assigned a time to a participant, data is an AssignmentDTO
        CreateRace = 7 //Admin created a race, data is a race
    }

    public enum ViewerCommands
    {
        //SERVER -> VIEWER
        State = 0, //The state of the current race, data is the state
        RaceStart = 1, //A race has started, data is the start time and all participants
        ParticipantFinished = 2, //A participant has finished, data is a participant who finished the race
        RaceEnd = 4, //The race has ended, data is null
        Races = 5, //Message contains all races up to this point, data is a list of races
        Participants = 6, //Message contains all participants for a race, data is a list of participants

        //VIEWER -> SERVER
        GetParticipants = 7 //Viewer is requesting all participants to a race, data is the race id
    }

    public enum ParticipantCommands
    {
        //SERVER -> PARTICIPANT
        Races = 0, //A list of all races of the future, the participant can register for, data is list of races

        //PARTICIPANT -> SERVER
        Register = 1 //A person wants to register, data is a participant
    }
}