using System.Net.WebSockets;
using System.Threading.Tasks;
using TimeMeasurement_Backend.Handlers.Messaging;

namespace TimeMeasurement_Backend.Handlers
{
    /// <summary>
    /// Handles websocket connection with a station
    /// </summary>
    public class StationHandler : Handler<StationCommands>
    {
        /// <summary>
        /// The weboscket corresponding to the station
        /// </summary>
        private WebSocket _station;

        public static StationHandler Instance { get; } = new StationHandler();

        /// <summary>
        /// Send a start signal to the station to tell it to start measuring the time
        /// </summary>
        public void SendStartSignal()
        {
            //Construct start signal message
            var toSend = new Message<StationCommands>
            {
                Command = StationCommands.Start,
                Data = null //No data
            };
            //Send in task because of async
            Task.Run(async () => await SendMessageAsync(_station, toSend));
        }

        /// <summary>
        /// Connect with station and listen for its messages
        /// </summary>
        /// <param name="ws">The Websocket corresponding to the station</param>
        /// <returns></returns>
        public async Task SetStationAsync(WebSocket ws)
        {
            //Something has already connected as a station
            if (_station != null)
            {
                return;
            }

            _station = ws;
            await ListenAsync(_station);
        }

        protected override void HandleMessage(WebSocket sender, Message<StationCommands> received)
        {
            switch (received.Command)
            {
                case StationCommands.StartTime:
                    //TODO Station has sent start time
                    break;
                case StationCommands.EndTime:
                    //TODO Station has sent end time
                    break;
                default:
                    //Command does not exist
                    return;
            }
        }
    }
}