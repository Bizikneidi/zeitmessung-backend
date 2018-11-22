using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TimeMeasurement_Backend.Logic;
using TimeMeasurement_Backend.Networking.Messaging;

namespace TimeMeasurement_Backend.Networking
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

        public StationHandler() => RaceManager.Instance.StateChanged += OnRaceManagerStateChanged;

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
            RaceManager.Instance.Ready(); //The RaceManager is now ready to start a race
            await ListenAsync(_station);
        }

        protected override void HandleMessage(WebSocket sender, Message<StationCommands> received)
        {
            switch (received.Command)
            {
                case StationCommands.MeasuredStart: //Station has played a tone and recorded the start time
                    RaceManager.Instance.TimeMeter.StartMeasurements((long)received.Data);
                    RaceManager.Instance.Start();
                    break;
                case StationCommands.MeasuredStop: //Station has detected that someone finished and recorded the end time
                    RaceManager.Instance.TimeMeter.StopMeasurement((long)received.Data);
                    break;
                default:
                    //Command does not exist
                    return;
            }
        }

        protected override void OnDisconnect(WebSocket disconnected)
        {
            RaceManager.Instance.Disable(); //The RaceManager is no longer ready to start a race
            _station = null;
        }

        private void OnRaceManagerStateChanged(RaceManager.State prev, RaceManager.State current)
        {
            //Only act, if Race Manager has requested the start of a race
            if (current != RaceManager.State.StartRequested)
            {
                return;
            }

            //Tell station to play a tone and start measuring
            var toSend = new Message<StationCommands>
            {
                Command = StationCommands.StartMeasuring,
                Data = RaceManager.Instance.Runners.Count()
            };
            Task.Run(async () => await SendMessageAsync(_station, toSend));
        }
    }
}