using System;
using System.Collections.Generic;
using System.Linq;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Persistence;

namespace TimeMeasurement_Backend.Logic
{
    /// <summary>
    /// Allows for a race state management, fires events when participants finish, a race ends, ...
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

        //Repos for database access
        private readonly TimeMeasurementRepository<Race> _raceRepo;

        /// <summary>
        /// The current race, which is being managed
        /// </summary>
        private Race _currentRace;

        /// <summary>
        /// The state of the current race
        /// </summary>
        private State _currentState;

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
        /// All races of the future
        /// </summary>
        public IEnumerable<Race> FutureRaces
        {
            get
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                return _raceRepo.Get(r => (_currentRace == null || r.Id != _currentRace.Id) && r.Done == false && r.Date > now);
            }
        }


        /// <summary>
        /// All races that can be started at the moment
        /// </summary>
        public IEnumerable<Race> StartableRaces
        {
            get
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                long twelveHours = 12 * 60 * 60 * 1000;
                return _raceRepo.Get(r => (_currentRace == null || r.Id != _currentRace.Id) && r.Done == false && r.Date >= now - twelveHours && r.Date <= now + twelveHours);
            }
        }

        /// <summary>
        /// All races of the past
        /// </summary>
        public IEnumerable<Race> PastRaces
        {
            get
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                return _raceRepo.Get(r => (_currentRace == null || r.Id != _currentRace.Id) && r.Done && r.Date < now);
            }
        }

        private RaceManager()
        {
            _currentState = State.Disabled;
            _raceRepo = new TimeMeasurementRepository<Race>();
        }

        /// <summary>
        /// Event to allow others to act accoring to the current state of the time meter
        /// </summary>
        public event Action<State, State> StateChanged;

        /// <summary>
        /// Adds a race to the database
        /// </summary>
        /// <param name="toCreate">The race to create</param>
        public void CreateRace(Race toCreate)
        {
            if (toCreate == null)
            {
                return;
            }

            _raceRepo.Create(toCreate);
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
        public void RequestStart(int raceId)
        {
            if (CurrentState != State.Ready)
            {
                return;
            }

            //Only allow start of races that can be started today
            if (StartableRaces.All(r => r.Id != raceId))
            {
                return;
            }

            _currentRace = _raceRepo.Get(r => r.Id == raceId).FirstOrDefault();
            if (_currentRace == null)
            {
                return;
            }

            CurrentState = State.StartRequested;
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

            CurrentState = State.InProgress;
        }
    }
}