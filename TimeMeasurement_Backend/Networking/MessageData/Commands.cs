namespace TimeMeasurement_Backend.Networking.MessageData
{
    public enum StationCommands
    {
        //SERVER -> STATION
        ///Station should start measuring times, data is null
        StartMeasuring = 0, 
        ///Station should stop measuring times, data is null
        StopMeasuring = 1, 

        //STATION -> SERVER
        ///Station has started measuring, data is the start time
        MeasuredStart = 2, 
        ///Station has measured a time, data is a stop time
        MeasuredStop = 3 
    }

    public enum AdminCommands
    {
        //SERVER -> ADMIN
        ///The state of the current race, data is the state
        State = 0, 
        ///All available races the admin can start, data is a list of races
        AvailableRaces = 1, 
        ///A race has started, data is a RaceStartDTO
        RaceStart = 2, 
        ///A participant has finished, data is the corresponding stop time
        MeasuredStop = 3, 
        ///The race has ended, data is null
        RaceEnd = 4, 

        //ADMIN -> SERVER
        ///Admin has pressed the start button and server should start a race. data is the id of a race
        Start = 5, 
        ///Admin assigned a time to a participant, data is an AssignmentDTO
        AssignTime = 6, 
        ///Admin created a race, data is a race
        CreateRace = 7 
    }

    public enum ViewerCommands
    {
        //SERVER -> VIEWER
        ///The state of the current race, data is the state
        State = 0, 
        ///A race has started, data is the start time and all participants
        RaceStart = 1, 
        ///A participant has finished, data is a participant who finished the race
        ParticipantFinished = 2, 
        ///The race has ended, data is null
        RaceEnd = 4, 
        ///Message contains all races up to this point, data is a list of races
        Races = 5, 
        ///Message contains all participants for a race, data is a list of participants
        Participants = 6, 

        //VIEWER -> SERVER
        ///Viewer is requesting all participants to a race, data is the race id
        GetParticipants = 7
    }

    public enum ParticipantCommands
    {
        //SERVER -> PARTICIPANT
        ///A list of all races of the future, the participant can register for, data is list of races
        Races = 0, 

        //PARTICIPANT -> SERVER
        ///A person wants to register, data is a participant
        Register = 1
    }
}