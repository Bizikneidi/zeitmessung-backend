using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Persistence;
using System.Linq;
using System;
using System.Collections.Generic;

namespace TimeMeasurement_Backend.Logic
{
    class ParticipantManager
    {
        private TimeMeasurementRepository<Participant> ParticipantRepo { get; } = new TimeMeasurementRepository<Participant>();

        /// <summary>
        /// Event to allow others to check, whenever a participant finishes the race
        /// </summary>
        public event Action<Participant> ParticipantFinished;

        private readonly TimeMeter _timeMeter = TimeMeter.Instance;
        private readonly RaceManager _raceManager = RaceManager.Instance;

        //all not yet assigned measurements
        private readonly List<long> _measurements;

        /// <summary>
        /// All participants taking part in the current race
        /// </summary>
        public IEnumerable<Participant> CurrentParticipants => GetParticipants(_raceManager.CurrentRace.Id);

        /// <summary>
        /// All recorded measurements which are not assigned to a participant yet
        /// </summary>
        public IEnumerable<long> UnassignedMeasurements => _measurements;

        private ParticipantManager() {
            _measurements = new List<long>();
            _timeMeter.OnMeasurement += measurement => _measurements.Add(measurement);
        }
        public static ParticipantManager Instance { get; } = new ParticipantManager();

        /// <summary>
        /// Assign an unique starter to the participant and store it to the db
        /// </summary>
        /// <param name="participant">The participant to store</param>
        public void AddParticipant(Participant participant)
        {
            int lastStarter = ParticipantRepo.Get().Select(p => p.Starter).Max();
            participant.Starter = lastStarter + 1;
            ParticipantRepo.Create(participant);
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
            var participant = ParticipantRepo.Get(r => r.Starter == starter && r.Race.Id == _raceManager.CurrentRace.Id, r => r.Race).FirstOrDefault();
            if (participant == null)
            {
                return false;
            }

            //Assign the time
            _measurements.Remove(time);
            participant.Time = time;
            ParticipantRepo.Update(participant);
            ParticipantFinished?.Invoke(participant);

            //Every participant has finished
            // ReSharper disable once InvertIf
            if (CurrentParticipants.All(r => r.Time != 0))
            {
                _raceManager.CurrentRace = null;
                _raceManager.CurrentState = RaceManager.State.Ready;
            }

            return true;
        }

        /// <summary>
        /// Get all participants to a specific race
        /// </summary>
        /// <param name="raceId">the id of the race</param>
        /// <returns>all participants who took part in the race</returns>
        public IEnumerable<Participant> GetParticipants(int raceId)
        {
            return ParticipantRepo.Get(r => r.Race.Id == raceId, r => r.Race);
        }
    }
}