using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Networking.Messaging;
using TimeMeasurement_Backend.Persistence;

namespace TimeMeasurement_Backend.Networking
{
    /// <summary>
    /// Handles websocket connection with an admin
    /// </summary>
    public class ParticipantHandler : Handler<ParticipantCommands>
    {
        /// <summary>
        /// Repository to store all new participants
        /// </summary>
        private readonly TimeMeasurementRepository<Participant> _repo;

        public ParticipantHandler() => _repo = new TimeMeasurementRepository<Participant>();

        /// <summary>
        /// Connect with a potential participant and listen for his/her messages
        /// </summary>
        /// <param name="participant">The websocket corresponding to a potential participant</param>
        /// <returns></returns>
        public async Task AddPotentialParticipant(WebSocket participant)
        {
            await ListenAsync(participant);
        }

        protected override void HandleMessage(WebSocket sender, Message<ParticipantCommands> received)
        {
            if (received.Command != ParticipantCommands.Register)
            {
                return;
            }

            //Deserialize received Participant and store in DB
            var participant = ((JObject)received.Data).ToObject<Participant>();
            _repo.Create(participant);
        }

        protected override void OnDisconnect(WebSocket disconnected)
        {
            //Nothing to clean up here!
        }
    }
}