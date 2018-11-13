using System;
using TimeMeasurement_Backend.Entities;

namespace TimeMeasurement_Backend.Logic
{
    /// <summary>
    /// A class to record a single time measurement
    /// </summary>
    public class TimeMeter
    {
        /// <summary>
        /// The internal time of the machine, at the time of StartMeasurements(starttime)
        /// </summary>
        private long _serverStartTime;

        /// <summary>
        /// The internal time of the station, at the time of StartMeasurements(starttime)
        /// </summary>
        private long _stationStartTime;

        /// <summary>
        /// Event gets fired, whenever another time has been measured
        /// </summary>
        public event Action<Time> OnMeasurement;

        /// <summary>
        /// Calculate the time of the station based on the difference between
        /// station start time and machine start time
        /// </summary>
        public long ApproximatedCurrentTime
        {
            get
            {
                long diff = _serverStartTime - _stationStartTime;
                return DateTimeOffset.Now.ToUnixTimeMilliseconds() - diff;
            }
        }

        /// <summary>
        /// State a measurement
        /// </summary>
        /// <param name="startTime">the start time of the measurement</param>
        public void StartMeasurements(long startTime)
        {
            //store current system time and station time
            _stationStartTime = startTime;
            _serverStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        
        /// <summary>
        /// Stop a measurement
        /// </summary>
        /// <param name="endTime">the stop time of the measurement</param>
        public void StopMeasurement(long endTime)
        {
            OnMeasurement?.Invoke(new Time
            {
                Start = _stationStartTime,
                End = endTime
            });
        }
    }
}