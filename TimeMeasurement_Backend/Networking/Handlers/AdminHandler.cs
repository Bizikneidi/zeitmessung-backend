using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TimeMeasurement_Backend.Logic;
using TimeMeasurement_Backend.Networking.MessageData;

namespace TimeMeasurement_Backend.Networking.Handlers
{
    /// <summary>
    /// Handles a websocket connection with an admin
    /// </summary>
    public class AdminHandler : Handler<AdminCommands>
    {
        /// <summary>
        /// The weboscket corresponding to the admin
        /// </summary>
        private WebSocket _admin;

        public AdminHandler()
        {
            RaceManager.Instance.StateChanged += OnRaceManagerStateChanged;
            RaceManager.Instance.TimeMeter.OnMeasurement += SendMeasuredStop;
        }

        /// <summary>
        /// Connect with admin and listen for his/her messages
        /// </summary>
        /// <param name="admin">The websocket corresponding to the admin</param>
        /// <returns></returns>
        public async Task SetAdminAsync(WebSocket admin)
        {
            //Someone has already connected as admin
            if (_admin != null)
            {
                return;
            }

            _admin = admin;
            SendCurrentState();
            if (RaceManager.Instance.CurrentState == RaceManager.State.InProgress)
            {
                SendRaceStart();
            }

            await ListenAsync(_admin);
        }

        protected override void HandleMessage(WebSocket sender, Message<AdminCommands> received)
        {
            switch (received.Command)
            {
                //Admin has pressed start
                case AdminCommands.Start:
                    RaceManager.Instance.RequestStart();
                    break;
                //Admin has assigned a time to a runner
                case AdminCommands.AssignTime:
                    var assignment = ((JObject)received.Data).ToObject<AssignmentDTO>();
                    //Try to assign the time
                    if (!RaceManager.Instance.TryAssignTimeToRunner(assignment.Starter, assignment.Time))
                    {
                        //something went wrong => resend time
                        SendMeasuredStop(assignment.Time);
                    }

                    break;
            }
        }

        protected override void OnDisconnect(WebSocket disconnected)
        {
            _admin = null;
        }

        private void OnRaceManagerStateChanged(RaceManager.State prev, RaceManager.State current)
        {
            SendCurrentState();

            if (current == RaceManager.State.InProgress)
            {
                SendRaceStart();
            }

            if (prev == RaceManager.State.InProgress)
            {
                SendRaceEnd();
            }
        }

        /// <summary>
        /// Send the current race state to the admin
        /// </summary>
        private void SendCurrentState()
        {
            var toSend = new Message<AdminCommands>
            {
                Command = AdminCommands.State,
                Data = RaceManager.Instance.CurrentState
            };
            SendMessage(_admin, toSend);
        }

        /// <summary>
        /// Sends a measurement to the admin to assign to a start number
        /// </summary>
        /// <param name="time">the measured time in ms</param>
        private void SendMeasuredStop(long time)
        {
            var message = new Message<AdminCommands>
            {
                Command = AdminCommands.MeasuredStop,
                Data = time
            };
            SendMessage(_admin, message);
        }

        /// <summary>
        /// Notify the admin that a race has ended
        /// </summary>
        private void SendRaceEnd()
        {
            var toSend = new Message<AdminCommands>
            {
                Command = AdminCommands.RaceEnd,
                Data = null
            };
            SendMessage(_admin, toSend);
        }

        /// <summary>
        /// Notify the admin that a race has started
        /// </summary>
        private void SendRaceStart()
        {
            var message = new Message<AdminCommands>
            {
                Command = AdminCommands.RaceStart,
                Data = new RaceStartDTO
                {
                    StartTime = RaceManager.Instance.TimeMeter.StartTime,
                    CurrentTime = RaceManager.Instance.TimeMeter.ApproximatedCurrentTime,
                    Runners = RaceManager.Instance.CurrentRunners
                }
            };
            SendMessage(_admin, message);

            //Also send all not yet assigned times
            foreach (long measurement in RaceManager.Instance.UnassignedMeasurements)
            {
                SendMeasuredStop(measurement);
            }
        }
    }
}