namespace TimeMeasurement_Backend.Handlers.Messaging
{
    public enum StationCommands
    {
        Start, //Station should start measuring the time
        StartTime, //Station sent the start time
        EndTime //Station sent the end time
    }

    public enum AdminCommands
    {
        Start //Admin has pressed the start button and server should start the run
    }

    public enum ViewerCommands
    {
        RunStart, //Broadcast that a run has started
        RunEnd //Broadcast that a run has ended
    }
}