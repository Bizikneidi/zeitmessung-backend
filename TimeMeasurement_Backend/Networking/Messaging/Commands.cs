namespace TimeMeasurement_Backend.Networking.Messaging
{
    public enum StationCommands
    {
        StartMeasuring = 0, //Station should start measuring the time
        MeasuredStart = 1, //Message contains the start time
        MeasuredStop = 2 //Message contains the stop time
    }

    public enum AdminCommands
    {
        Status = 0, //Message contains the current time measurement status
        Start = 1 //Admin has pressed the start button and server should start a measurement
    }

    public enum ViewerCommands
    {
        Status = 0, //Message contains the current time measurement status
        MeasuredStart = 1, //Broadcast where message contains the start time
        MeasuredEnd = 2 //Broadcast where message contains the stop time
    }
}