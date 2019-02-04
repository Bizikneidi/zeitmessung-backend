using System;
using System.Collections.Generic;
using System.Linq;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Persistence;

namespace TimeMeasurement_Backend.Logic
{
    /// <summary>
    /// Allows for race state management, fires events when participants finishes, a race ends, ...
    /// </summary>
    public class RaceManager
    {
        /// <summary>
        /// The possible states of the race
        /// </summary>
        public enum State
        {
            Ready, //The racemanager is theoretically ready to start a race (A station has connected)
            StartRequested, //The admin requested the start of a race (The admin has pressed start)
            InProgress, //A race is in progress
            Disabled //The racemanager is not ready and nobody can request or start a race (no station is connected)
        }

        ///Repo for database access
        private readonly TimeMeasurementRepository<Race> _raceRepo;

        private State _currentState;

        /// <summary>
        /// The current, active race
        /// </summary>
        public Race CurrentRace { get; private set; }

        /// <summary>
        /// The current state of the active race
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
        /// All races which take place in the future
        /// </summary>
        public IEnumerable<Race> FutureRaces
        {
            get
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                //Note: Races of the future can be started in a 12 hour margin.
                return _raceRepo.Get(r => (CurrentRace == null || r.Id != CurrentRace.Id) && r.Done == false && r.Date > now);
            }
        }

        ///Singleton
        public static RaceManager Instance { get; } = new RaceManager();

        /// <summary>
        /// All races that have been done
        /// </summary>
        public IEnumerable<Race> PastRaces
        {
            get
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                return _raceRepo.Get(r => (CurrentRace == null || r.Id != CurrentRace.Id) && r.Done && r.Date < now);
            }
        }

        /// <summary>
        /// All races that can be started at the moment
        /// </summary>
        public IEnumerable<Race> StartableRaces
        {
            get
            {
                return _raceRepo.Get(r =>
                    (CurrentRace == null || r.Id != CurrentRace.Id)
                    && r.Done == false
                    && DateTimeOffset.Now.Date == DateTimeOffset.FromUnixTimeMilliseconds(r.Date).ToUniversalTime().Date
                );
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
            AbortRace();
            CurrentState = State.Disabled;
        }

        /// <summary>
        /// Ends the currently active race
        /// </summary>
        public void CompleteRace()
        {
            if (CurrentRace == null)
            {
                return;
            }

            CurrentRace.Done = true;
            _raceRepo.Update(CurrentRace);
            CurrentRace = null;
            CurrentState = State.Ready;
        }

        /// <summary>
        /// Ends the currently active race
        /// </summary>
        public void AbortRace()
        {
            if (CurrentRace == null)
            {
                return;
            }

            CurrentRace = null;
            CurrentState = State.Ready;
        }

        /// <summary>
        /// Allows the station to set the State to ready
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
        /// Allows others to set the State to StartRequested
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

            CurrentRace = _raceRepo.Get(r => r.Id == raceId).FirstOrDefault();
            if (CurrentRace == null)
            {
                return;
            }

            CurrentState = State.StartRequested;
        }

        /// <summary>
        /// Allows others to set the State to InProgress (Starts a race)
        /// </summary>
        public void Start()
        {
            if (CurrentState != State.StartRequested)
            {
                return;
            }

            CurrentRace.Date = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            CurrentState = State.InProgress;

            if (!ParticipantManager.Instance.CurrentParticipants.Any())
            {
                AbortRace();
            }
        }
    }
}