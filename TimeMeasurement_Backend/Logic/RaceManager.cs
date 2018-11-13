namespace TimeMeasurement_Backend.Logic
{
    public class RaceManager
    {
        /// <summary>
        /// The possible states of the time meter
        /// </summary>
        public enum State
        {
            Ready, //The time meter is theoretically ready to start a measurement
            StartRequested, //The admin sent a start
            InProgress, //The race is in progress
            Disabled //The time meter can not start a measurement and nobody can request one
        }
    }
}