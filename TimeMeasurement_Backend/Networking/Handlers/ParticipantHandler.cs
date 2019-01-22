using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Logic;
using TimeMeasurement_Backend.Networking.MessageData;

namespace TimeMeasurement_Backend.Networking.Handlers
{
    /// <summary>
    /// Handles a websocket connection with a potential participant
    /// </summary>
    public class ParticipantHandler : Handler<ParticipantCommands>
    {
        /// <summary>
        /// Logic to manage Participants
        /// </summary>
        private readonly ParticipantManager _participantManager = ParticipantManager.Instance;


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

            //Deserialize received Participant, assign starter and store in DB
            _participantManager.AddParticipant(((JObject)received.Data).ToObject<Participant>());
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
            var message = new Message<ParticipantCommands>
            {
                Command = ParticipantCommands.Races,
                Data = RaceManager.Instance.FutureRaces
            };
            SendMessage(participant, message);
        }
    }
}