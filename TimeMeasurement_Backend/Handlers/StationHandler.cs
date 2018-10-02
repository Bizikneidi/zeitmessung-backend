using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Handlers.Messaging;
using TimeMeasurement_Backend.Persistence;

namespace TimeMeasurement_Backend.Handlers
{
    /// <summary>
    /// Handles websocket connection with a station
    /// </summary>
    public class StationHandler : Handler<StationCommands>
    {
        /// <summary>
        /// The repo for storing time entities
        /// </summary>
        private readonly TimeMeasurementRepository<Time> _timeRepo;

        /// <summary>
        /// The current time that's being measured
        /// </summary>
        private Time _currentTime;

        /// <summary>
        /// The weboscket corresponding to the station
        /// </summary>
        private WebSocket _station;

        public static StationHandler Instance { get; } = new StationHandler();

        private StationHandler()
        {
            _timeRepo = new TimeMeasurementRepository<Time>();
            _currentTime = null;
        }

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
                    SetupCurrentTime((DateTime)received.Data);
                    break;
                case StationCommands.EndTime:
                    EndCurrentTime((DateTime)received.Data);
                    break;
                default:
                    //Command does not exist
                    return;
            }
        }

        protected override void OnDisconnect(WebSocket disconnected)
        {
            //Station has disconnected => time has to be reset as well
            _station = null;
            _currentTime = null;
        }

        /// <summary>
        /// Ends the current time, stores it in db and sends it to all viewers
        /// </summary>
        /// <param name="end">the time the run has ended</param>
        private void EndCurrentTime(DateTime end)
        {
            //Time is not being measured
            if (_currentTime == null)
            {
                return;
            }

            //Apply end time and store in db
            _currentTime.End = end;
            _timeRepo.Create(_currentTime);

            //reset time
            _currentTime = null;

            //Tell all viewers that a run has ended
            ViewerHandler.Instance.BroadcastRunEnd(end);
        }

        /// <summary>
        /// Creates new time and sends it to all viewers
        /// </summary>
        /// <param name="start">the time the run has started</param>
        private void SetupCurrentTime(DateTime start)
        {
            //Time is already being measured
            if (_currentTime != null)
            {
                return;
            }

            _currentTime = new Time
            {
                Start = start,
                End = null
            };

            //Tell all viewers that a run has started
            ViewerHandler.Instance.BroadcastRunStart(start);
        }
    }
}