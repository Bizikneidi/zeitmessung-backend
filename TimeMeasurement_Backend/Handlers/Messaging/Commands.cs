namespace TimeMeasurement_Backend.Handlers.Messaging
{
    public enum StationCommands
    {
        Start, //Station should start measuring time
        StartTime, //Message contains start time
        EndTime //Message contains end time
    }

    public enum AdminCommands
    {
        Start //Server should start the run
    }

    public enum ViewerCommands
    {
        //TODO Add Commands
    }
}