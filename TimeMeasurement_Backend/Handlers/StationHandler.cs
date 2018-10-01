using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TimeMeasurement_Backend.Handlers
{
    /// <summary>
    /// Handles websocket connection with a station
    /// </summary>
    public class StationHandler : Handler<StationHandler.Commands>
    {
        public enum Commands
        {
            Start, //Station should start measuring time
            StartTime, //Message contains start time
            EndTime //Message contains end time
        }

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
            var toSend = new Message<Commands>
            {
                Command = Commands.Start,
                Data = null //No data
            };
            //Send in task because of async
            Task.Run(() => SendMessageAsync(_station, toSend));
        }

        /// <summary>
        /// Connect with station and listen for its messages
        /// </summary>
        /// <param name="ws">The Websocket corrresponding to the station</param>
        /// <returns></returns>
        public async Task SetStation(WebSocket ws)
        {
            //Something has already connected as a station
            if (_station != null)
            {
                return;
            }

            _station = ws;
            await ListenAsync(_station);
        }

        protected override void HandleMessage(WebSocket sender, Message<Commands> received)
        {
            switch (received.Command)
            {
                case Commands.StartTime:
                    //TODO Station has sent start time
                    break;
                case Commands.EndTime:
                    //TODO Station has sent end time
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}