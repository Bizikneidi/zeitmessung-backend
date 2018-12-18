using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Logic;
using TimeMeasurement_Backend.Networking.MessageData;
using TimeMeasurement_Backend.Persistence;

namespace TimeMeasurement_Backend.Networking.Handlers
{
    /// <summary>
    /// Handles a websocket connection with a potential participant
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
            SendRaces(participant);
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

        /// <summary>
        /// Sends the list of races of the furutre to a participant
        /// </summary>
        /// <param name="participant">The Websocket corresponding to a potential participant</param>
        private void SendRaces(WebSocket participant)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var message = new Message<ParticipantCommands>
            {
                Command = ParticipantCommands.Races,
                Data = RaceManager.Instance.Races.Where(r => r.Date > now)
            };
            SendMessage(participant, message);
        }
    }
}