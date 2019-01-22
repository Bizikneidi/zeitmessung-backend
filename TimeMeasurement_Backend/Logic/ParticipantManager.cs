using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Persistence;
using System.Linq;
using System;
using System.Collections.Generic;

namespace TimeMeasurement_Backend.Logic
{
    /// <summary>
    /// Manages participants
    /// </summary>
    class ParticipantManager
    {
        public static ParticipantManager Instance { get; } = new ParticipantManager();

        private TimeMeasurementRepository<Participant> ParticipantRepo { get; }

        /// <summary>
        /// Event to allow others to check, whenever a participant finishes a race
        /// </summary>
        public event Action<Participant> ParticipantFinished;

        //all not yet assigned measurements
        private readonly List<long> _measurements;

        /// <summary>
        /// All participants taking part in the current race
        /// </summary>
        public IEnumerable<Participant> CurrentParticipants => GetParticipants(RaceManager.Instance.CurrentRace.Id);

        /// <summary>
        /// All recorded measurements which are not assigned to a participant yet
        /// </summary>
        public IEnumerable<long> UnassignedMeasurements => _measurements;

        private ParticipantManager()
        {
            _measurements = new List<long>();
            TimeMeter.Instance.OnMeasurement += measurement => _measurements.Add(measurement);
            ParticipantRepo = new TimeMeasurementRepository<Participant>();
        }

        /// <summary>
        /// Assign an unique starter to the participant and store it to the db
        /// </summary>
        /// <param name="participant">The participant to store</param>
        public void AddParticipant(Participant participant)
        {
            int lastStarter = ParticipantRepo.Get(p => p.Race.Id == participant.Race.Id, p => p.Race)?.Select(p => p.Starter).Count() ?? 0;
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
            var participant = ParticipantRepo.Get(r => r.Starter == starter && r.Race.Id == RaceManager.Instance.CurrentRace.Id && r.Time == 0, r => r.Race).FirstOrDefault();
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
                RaceManager.Instance.FinishRace();
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