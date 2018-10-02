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
        /// The current state of the time meter
        /// </summary>
        public State CurrentState
        {
            get => _currentState;
            set
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
        /// State a measurement
        /// </summary>
        /// <param name="startTime">the start time of the measurement</param>
        public void StartMeasurement(long startTime)
        {
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