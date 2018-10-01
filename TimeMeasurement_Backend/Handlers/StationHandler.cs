using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TimeMeasurement_Backend.Handlers
{
    /// <summary>
    /// Handles websocket connection with a station
    /// </summary>
    public class StationHandler : Handler<StationHandler.Command>
    {
        public enum Command
        {
            Start, //Station should start measuring time
            StartTime, //Message contains start time
            EndTime //Message contains end time
        }

        public static StationHandler Instance { get; } = new StationHandler();

        private WebSocket _station;

        /// <summary>
        /// Send a start signal to the station to tell it to start measuring the time
        /// </summary>
        public void SendStartSignal()
        {
            //Construct start signal message
            var toSend = new Message<Command>
            {
                Command = Command.Start,
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
            _station = ws;
            await ListenAsync(_station);
        }

        protected override void HandleMessage(WebSocket sender, Message<Command> received)
        {
            switch (received.Command)
            {
                case Command.StartTime:
                    //Station has sent start time
                    break;
                case Command.EndTime:
                    //Station has sent end time
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
