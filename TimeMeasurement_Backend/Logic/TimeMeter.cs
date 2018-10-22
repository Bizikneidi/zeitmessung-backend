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
        /// The possible states of the time meter
        /// </summary>
        public enum State
        {
            Ready, //The time meter is theoretically ready to start a measurement
            MeasurementRequested, //Something has requested the time meter to start a measurement
            Measuring, //The time meter is currently measuring a time
            Disabled //The time meter can not start a measurement and nobody can request one
        }

        private State _currentState;

        /// <summary>
        /// The internal time of the machine, at the time of StartMeasurement(starttime)
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
                //Can not calculate, if no time is being measured
                if (CurrentState != State.Measuring || Measurement.Start == null)
                {
                    return -1;
                }

                long diff = _serverStartTime - (long)Measurement.Start;
                return DateTimeOffset.Now.ToUnixTimeMilliseconds() - diff;
            }
        }

        /// <summary>
        /// The current state of the time meter
        /// </summary>
        public State CurrentState
        {
            get => _currentState;
            private set
            {
                var prev = _currentState;
                _currentState = value;
                StateChanged?.Invoke(prev, _currentState); //Notify subscribers to act
            }
        }

        /// <summary>
        /// Singleton, since only one time can be measured at the moment
        /// </summary>
        public static TimeMeter Instance { get; } = new TimeMeter();

        /// <summary>
        /// The current recorded measurement (can be null)
        /// </summary>
        public Time Measurement { get; private set; }

        private TimeMeter() => _currentState = State.Disabled;

        /// <summary>
        /// Event to allow others to act accoring to the current state of the time meter
        /// </summary>
        public event Action<State, State> StateChanged;

        /// <summary>
        /// Allows others to set the time meter to disabled
        /// </summary>
        public void Disable()
        {
            Measurement = null;
            CurrentState = State.Disabled;
        }

        /// <summary>
        /// Allows others to set the time meter to ready
        /// </summary>
        public void Ready()
        {
            //Only possible, if time meter is disabled
            if (CurrentState == State.Disabled)
            {
                CurrentState = State.Ready;
            }
        }

        /// <summary>
        /// Allows others to request a measurement
        /// </summary>
        public void RequestMeasurement()
        {
            //Only possible, if time meter is ready
            if (CurrentState == State.Ready)
            {
                CurrentState = State.MeasurementRequested;
            }
        }

        /// <summary>
        /// State a measurement
        /// </summary>
        /// <param name="startTime">the start time of the measurement</param>
        public void StartMeasurement(long startTime)
        {
            //store current system time
            _serverStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Measurement = new Time
            {
                Start = startTime,
                End = null
            };
            CurrentState = State.Measuring; //Has started measuring
        }

        /// <summary>
        /// Stop a measurement
        /// </summary>
        /// <param name="endTime">the stop time of the measurement</param>
        public void StopMeasurement(long endTime)
        {
            Measurement.End = endTime;
            CurrentState = State.Ready; //Has ended measuring and is again ready
        }
    }
}