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

        public StationHandler() => TimeMeter.Instance.StateChanged += OnTimeMeterStateChanged;

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
            TimeMeter.Instance.Ready(); //The TimeMeter is now ready to start a measurement
            TimeMeter.Instance.RequestMeasurement();
            await ListenAsync(_station);
        }

        protected override void HandleMessage(WebSocket sender, Message<StationCommands> received)
        {
            switch (received.Command)
            {
                case StationCommands.MeasuredStart: //Station has played a tone and recorded the start time
                    TimeMeter.Instance.StartMeasurement((long)received.Data);
                    break;
                case StationCommands.MeasuredStop: //Station has detected that someone finished and recorded the end time
                    TimeMeter.Instance.StopMeasurement((long)received.Data);
                    break;
                default:
                    //Command does not exist
                    return;
            }
        }

        

        protected override void OnDisconnect(WebSocket disconnected)
        {
            TimeMeter.Instance.Disable(); //The TimeMeter is no longer ready to start a measurement
            _station = null;
        }

        private void OnTimeMeterStateChanged(TimeMeter.State prev, TimeMeter.State current)
        {
            //Only act, if timemeter has requested the start of a measurement
            if (current != TimeMeter.State.MeasurementRequested)
            {
                return;
            }

            //Tell station to play a tone and start measuring
            var toSend = new Message<StationCommands>
            {
                Command = StationCommands.StartMeasuring,
                Data = null
            };
            Task.Run(async () => await SendMessageAsync(_station, toSend));
        }
    }
}