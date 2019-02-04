using System;

namespace TimeMeasurement_Backend.Logic
{
    /// <summary>
    /// Allows keeping track of the time and measuring times
    /// </summary>
    public class TimeMeter
    {
        /// <summary>
        /// The internal time of the server, at the time of StartMeasurements(starttime)
        /// </summary>
        private long _serverStartTime;

        /// <summary>
        /// Calculate the time of the station based on the difference between
        /// station start time and machine start time
        /// </summary>
        public long ApproximatedCurrentTime
        {
            get
            {
                long diff = _serverStartTime - StartTime; //Diff between server time and station time
                return DateTimeOffset.Now.ToUnixTimeMilliseconds() - diff; //Time passed since the start
            }
        }

        public static TimeMeter Instance { get; } = new TimeMeter();

        /// <summary>
        /// The internal time of the station, at the time of StartMeasurements(starttime)
        /// </summary>
        public long StartTime { get; private set; }

        private TimeMeter() { }

        /// <summary>
        /// Event gets fired, whenever another time has been measured
        /// </summary>
        public event Action<long> OnMeasurement;

        /// <summary>
        /// Start a measurement
        /// </summary>
        /// <param name="startTime">the station start time of the measurement</param>
        public void StartMeasurements(long startTime)
        {
            if (RaceManager.Instance.CurrentState != RaceManager.State.StartRequested)
            {
                return;
            }

            //store current system time and station time
            StartTime = startTime;
            _serverStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Stop a measurement
        /// </summary>
        /// <param name="endTime">the stop time of the measurement</param>
        public void StopMeasurement(long endTime)
        {
            if (endTime <= StartTime)
            {
                return;
            }

            OnMeasurement?.Invoke(endTime - StartTime);
        }
    }
}