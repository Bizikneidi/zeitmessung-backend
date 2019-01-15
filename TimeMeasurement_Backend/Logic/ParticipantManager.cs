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

        /// <summary>
        /// All participants taking part in the current race
        /// </summary>
        public IEnumerable<Participant> CurrentParticipants => GetParticipants(_currentRace.Id);

        private ParticipantManager() { }
        public static ParticipantManager Instance { get; } = new ParticipantManager();

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
            var participant = ParticipantRepo.Get(r => r.Starter == starter && r.Race.Id == _currentRace.Id, r => r.Race).FirstOrDefault();
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
                _currentRace = null;
                CurrentState = State.Ready;
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