using System;
using System.Collections.Generic;
using System.Linq;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Entities.Constraints;
using TimeMeasurement_Backend.Entities.Entities;
using TimeMeasurement_Backend.Persistence;

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

        private State _currentState;
        
        /// <summary>
        /// Event to allow others to act accoring to the current state of the time meter
        /// </summary>
        public event Action<State, State> StateChanged;

        private TimeMeter _timeMeter;
        private List<Time> _measurements;
        private TimeMeasurementRepository<Participant> _participantRepo;
        private IEnumerable<Runner> _runners;
        private TimeMeasurementRepository<Runner> _runnerRepo;
        private Race _currentRace;
        
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

        private RaceManager()
        {
            _currentState = State.Disabled;
            _timeMeter = new TimeMeter();
            _timeMeter.OnMeasurement += measurement => { _measurements.Add(measurement); };

            _participantRepo = new TimeMeasurementRepository<Participant>();
            _runnerRepo = new TimeMeasurementRepository<Runner>();
        }
        
        /// <summary>
        /// Allows others to set the time meter to disabled
        /// </summary>
        public void Disable()
        {
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
        /// Allows others to set the time meter to StartRequested
        /// </summary>
        public void RequestStart()
        {
            if (CurrentState == State.Ready)
            {
                CurrentState = State.StartRequested;
            }
        }

        /// <summary>
        /// Allows others to set the time meter to InProgress
        /// </summary>
        public void Start()
        {
            if (CurrentState == State.StartRequested)
            {
                //Pass to TimeMeter
                CurrentState = State.InProgress;
                RegisterRunners();
                _currentRace = new Race() { Date = DateTimeOffset.Now.ToUnixTimeMilliseconds()};
                new TimeMeasurementRepository<Race>().Create(_currentRace);
            }
        }

        /// <summary>
        /// Mapping all participants to runners (new object with a time entity)
        /// </summary>
        private void RegisterRunners()
        {
            foreach (var runner in GetNewParticipants().Select((p, i) => new Runner {Starter = i, Participant = p, Race = _currentRace}))
            {
                _runnerRepo.Create(runner);
            }
        }

        /// <summary>
        /// Querying through all participants, getting all where no runner exists
        /// </summary>
        /// <returns>new participants</returns>
        private IEnumerable<Participant> GetNewParticipants()
        {
            return _participantRepo.Get(p => !_runnerRepo.Get(r => r.Participant == p).Any());
        }

        /// <summary>
        /// Assigns time to a runner
        /// </summary>
        /// <param name="starter"></param>
        /// <param name="time"></param>
        private void AssignTimeToRunner(int starter, Time time)
        {
            var runner = _runnerRepo.Get(p => p.Starter == starter).First();
            runner.Time = time;
            _runnerRepo.Update(runner);
        }
    }
}