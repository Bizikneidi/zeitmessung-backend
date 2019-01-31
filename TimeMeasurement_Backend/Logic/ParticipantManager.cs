using System;
using System.Collections.Generic;
using System.Linq;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Persistence;

namespace TimeMeasurement_Backend.Logic
{
    /// <summary>
    /// Manages the current participants in a race, Allows to retreive participants for a certain race
    /// </summary>
    internal class ParticipantManager
    {
        private readonly List<long> _measurements;

        /// <summary>
        /// All participants taking part in the current race
        /// </summary>
        public IEnumerable<Participant> CurrentParticipants => GetParticipants(RaceManager.Instance.CurrentRace.Id);

        ///Singleton
        public static ParticipantManager Instance { get; } = new ParticipantManager();

        /// <summary>
        /// All recorded measurements which are not assigned to a participant yet
        /// </summary>
        public IEnumerable<long> UnassignedMeasurements => _measurements;

        ///Repository for Database Access
        private TimeMeasurementRepository<Participant> ParticipantRepo { get; }

        private ParticipantManager()
        {
            _measurements = new List<long>();
            TimeMeter.Instance.OnMeasurement += measurement => _measurements.Add(measurement);
            ParticipantRepo = new TimeMeasurementRepository<Participant>();
        }

        /// <summary>
        /// Event to allow others to get notified, whenever a participant finishes a race
        /// </summary>
        public event Action<Participant> ParticipantFinished;

        /// <summary>
        /// Assign an unique starter to the participant and store it in the db
        /// </summary>
        /// <param name="participant">The participant to register</param>
        public void AddParticipant(Participant participant)
        {
            //Participant must register for a future race
            if (!RaceManager.Instance.FutureRaces.Select(r => r.Id).Contains(participant.Race.Id))
                return;

            int lastStarter = ParticipantRepo.Get(p => p.Race.Id == participant.Race.Id, p => p.Race)?.Select(p => p.Starter).Count() ?? 0;
            participant.Starter = lastStarter + 1;
            ParticipantRepo.Create(participant);
        }

        /// <summary>
        /// Get all participants for a specific race
        /// </summary>
        /// <param name="raceId">the id of the race</param>
        /// <returns>all participants who took part in the race</returns>
        public IEnumerable<Participant> GetParticipants(int raceId)
        {
            return ParticipantRepo.Get(r => r.Race.Id == raceId, r => r.Race);
        }

        /// <summary>
        /// Assigns a time to a participant, if possible
        /// </summary>
        /// <param name="starter">the starter number of the participant</param>
        /// <param name="time">the time to assign to the participant</param>
        /// <returns>true, if the assignment was successful</returns>
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
            //Notify others
            ParticipantFinished?.Invoke(participant);

            //Every participant has finished => End Race
            // ReSharper disable once InvertIf
            if (CurrentParticipants.All(r => r.Time != 0))
            {
                RaceManager.Instance.CompleteRace();
                _measurements.Clear();
            }

            return true;
        }
    }
}