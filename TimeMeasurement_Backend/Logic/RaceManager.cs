using System;
using System.Collections.Generic;
using System.Linq;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Persistence;

namespace TimeMeasurement_Backend.Logic
{
    /// <summary>
    /// Allows for a race state management, fires events when runners finish, a race ends, ...
    /// </summary>
    public class RaceManager
    {
        /// <summary>
        /// The possible states of the race
        /// </summary>
        public enum State
        {
            Ready, //The racemanager is theoretically ready to start a race
            StartRequested, //The admin requested the start of a race
            InProgress, //A race is in progress
            Disabled //The racemanager is not ready and nobody can request or start a race
        }

        //all not assigned measurements
        private readonly List<long> _measurements;

        //Repos for database access
        private readonly TimeMeasurementRepository<Participant> _participantRepo;
        private readonly TimeMeasurementRepository<Race> _raceRepo;
        private readonly TimeMeasurementRepository<Runner> _runnerRepo;

        /// <summary>
        /// The current race, which is being managed
        /// </summary>
        private Race _currentRace;

        /// <summary>
        /// The state of the current race
        /// </summary>
        private State _currentState;

        /// <summary>
        /// All runners participating in the current race
        /// </summary>
        public IEnumerable<Runner> CurrentRunners => _runnerRepo.Get(r => r.Race.Id == _currentRace.Id, r => r.Race, r => r.Participant);

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

        public static RaceManager Instance { get; } = new RaceManager();

        /// <summary>
        /// All races that were ever monitored except the current one
        /// </summary>
        public IEnumerable<Race> Races => _currentRace == null ? _raceRepo.Get() : _raceRepo.Get(r => r.Id != _currentRace.Id);

        /// <summary>
        /// Keeps track of the time
        /// </summary>
        public TimeMeter TimeMeter { get; }

        /// <summary>
        /// All recorded measurements which are not assigned to a runner yet
        /// </summary>
        public IEnumerable<long> UnassignedMeasurements => _measurements;

        private RaceManager()
        {
            TimeMeter = new TimeMeter();
            TimeMeter.OnMeasurement += measurement => _measurements.Add(measurement);

            _measurements = new List<long>();

            _currentState = State.Disabled;
            _participantRepo = new TimeMeasurementRepository<Participant>();
            _runnerRepo = new TimeMeasurementRepository<Runner>();
            _raceRepo = new TimeMeasurementRepository<Race>();
        }

        /// <summary>
        /// Event to allow others to check, whenever a runner finishes the race
        /// </summary>
        public event Action<Runner> RunnerFinished;

        /// <summary>
        /// Event to allow others to act accoring to the current state of the time meter
        /// </summary>
        public event Action<State, State> StateChanged;

        /// <summary>
        /// Allows others to set the time meter to disabled
        /// </summary>
        public void Disable()
        {
            CurrentState = State.Disabled;
        }

        /// <summary>
        /// Get all runners to a specific race
        /// </summary>
        /// <param name="raceId">the id of the race</param>
        /// <returns>all runners who participated in the race</returns>
        public IEnumerable<Runner> GetRunners(int raceId)
        {
            return _runnerRepo.Get(r => r.Race.Id == raceId, r => r.Race, r => r.Participant);
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
            if (CurrentState != State.StartRequested)
            {
                return;
            }

            _measurements.Clear();
            _currentRace = new Race { Date = DateTimeOffset.Now.ToUnixTimeMilliseconds() };
            _raceRepo.Create(_currentRace);
            RegisterRunners();
            CurrentState = State.InProgress;
        }

        /// <summary>
        /// Assigns a time to the runner with the starter if possible
        /// </summary>
        /// <param name="starter">the starter number of the runner</param>
        /// <param name="time">the time to assign to the runner</param>
        /// <returns>if the assignment was successful</returns>
        public bool TryAssignTimeToRunner(int starter, long time)
        {
            //Prevent assigning faulty times to runners
            if (!_measurements.Contains(time))
            {
                return false;
            }

            _measurements.Remove(time);

            //Make sure a runner with the starter exists
            var runner = _runnerRepo.Get(r => r.Starter == starter && r.Race.Id == _currentRace.Id, r => r.Race).FirstOrDefault();
            if (runner == null)
            {
                return false;
            }

            //Assign the time
            runner.Time = time;
            _runnerRepo.Update(runner);
            RunnerFinished?.Invoke(runner);

            //Every Runner has finished
            if (CurrentRunners.All(r => r.Time != 0))
            {
                _currentRace = null;
                CurrentState = State.Ready;
            }

            return true;
        }

        /// <summary>
        /// Querying through all participants, getting all where no corresponding runner entity exists
        /// </summary>
        /// <returns>new participants</returns>
        private IEnumerable<Participant> GetNewParticipants()
        {
            var participants = _participantRepo.Get();
            var res = participants.Where(p => _runnerRepo.Get(r => r.Participant.Id == p.Id, r => r.Participant).FirstOrDefault() == null);
            return res;
        }

        /// <summary>
        /// Mapping all participants to runner entities
        /// </summary>
        private void RegisterRunners()
        {
            foreach (var runner in GetNewParticipants().Select((p, i) => new Runner
            {
                Starter = i,
                Participant = p,
                Race = _currentRace,
                Time = 0
            }))
            {
                _runnerRepo.Create(runner);
            }
        }
    }
}