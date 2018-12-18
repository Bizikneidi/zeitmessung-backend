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

        /// <summary>
        /// The current race, which is being managed
        /// </summary>
        private Race _currentRace;

        /// <summary>
        /// The id of the currently requested race, if valid
        /// </summary>
        private int _currentRaceId;

        /// <summary>
        /// The state of the current race
        /// </summary>
        private State _currentState;

        /// <summary>
        /// All participants taking part in the current race
        /// </summary>
        public IEnumerable<Participant> CurrentParticipants => GetParticipants(_currentRace.Id);

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
        /// All recorded measurements which are not assigned to a participant yet
        /// </summary>
        public IEnumerable<long> UnassignedMeasurements => _measurements;

        private RaceManager()
        {
            TimeMeter = new TimeMeter();
            TimeMeter.OnMeasurement += measurement => _measurements.Add(measurement);

            _measurements = new List<long>();

            _currentState = State.Disabled;
            _participantRepo = new TimeMeasurementRepository<Participant>();
            _raceRepo = new TimeMeasurementRepository<Race>();
        }

        /// <summary>
        /// Event to allow others to check, whenever a participant finishes the race
        /// </summary>
        public event Action<Participant> ParticipantFinished;

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
        /// Get all participants to a specific race
        /// </summary>
        /// <param name="raceId">the id of the race</param>
        /// <returns>all participants who took part in the race</returns>
        public IEnumerable<Participant> GetParticipants(int raceId)
        {
            return _participantRepo.Get(r => r.Race.Id == raceId, r => r.Race);
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

            CurrentState = State.StartRequested;
            _currentRaceId = raceId;
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
            _currentRace = _raceRepo.Get(r => r.Id == _currentRaceId).FirstOrDefault();
            CurrentState = State.InProgress;
        }

        /// <summary>
        /// Assigns a time to the participant with the starter if possible
        /// </summary>
        /// <param name="starter">the starter number of the participant</param>
        /// <param name="time">the time to assign to the participant</param>
        /// <returns>if the assignment was successful</returns>
        public bool TryAssignTimeToRunner(int starter, long time)
        {
            //Prevent assigning faulty times to participants
            if (!_measurements.Contains(time))
            {
                return false;
            }

            //Make sure a participant with the starter exists
            var participant = _participantRepo.Get(r => r.Starter == starter && r.Race.Id == _currentRace.Id, r => r.Race).FirstOrDefault();
            if (participant == null)
            {
                return false;
            }

            //Assign the time
            _measurements.Remove(time);
            participant.Time = time;
            _participantRepo.Update(participant);
            ParticipantFinished?.Invoke(participant);

            //Every Runner has finished
            // ReSharper disable once InvertIf
            if (CurrentParticipants.All(r => r.Time != 0))
            {
                _currentRace = null;
                CurrentState = State.Ready;
            }

            return true;
        }
    }
}