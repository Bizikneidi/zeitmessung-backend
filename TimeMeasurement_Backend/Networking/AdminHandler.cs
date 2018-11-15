using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TimeMeasurement_Backend.Logic;
using TimeMeasurement_Backend.Networking.Messaging;

namespace TimeMeasurement_Backend.Networking
{
    /// <summary>
    /// Handles websocket connection with an admin
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
            RaceManager.Instance.TimeMeter.OnMeasurement += OnTimeMeterMeasurement;
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
            await SendCurrentState();
            await ListenAsync(_admin);
        }

        protected override void HandleMessage(WebSocket sender, Message<AdminCommands> received)
        {
            switch (received.Command)
            {
                case AdminCommands.Start:
                    //Admin has pressed start
                    RaceManager.Instance.RequestStart();
                    break;
                case AdminCommands.AssignTime:
                    //Admin has mapped a runner to a time
                    var assignment = ((JObject)received.Data).ToObject<AssignmentDTO>();
                    RaceManager.Instance.AssignTimeToRunner(assignment.Starter, assignment.Time);
                    break;
            }
        }

        protected override void OnDisconnect(WebSocket disconnected)
        {
            _admin = null;
        }

        private void OnRaceManagerStateChanged(RaceManager.State prev, RaceManager.State current)
        {
            //Notify admin
            Task.Run(async () => await SendCurrentState());
        }

        private void OnTimeMeterMeasurement(long time)
        {
            //Send time to admin, to map to runner
            var message = new Message<AdminCommands>
            {
                Command = AdminCommands.MeasuredStop,
                Data = time
            };
            Task.Run(async () => await SendMessageAsync(_admin, message));
        }

        /// <summary>
        /// Tell the admin wether he is allowed to start a measurement / if not tell him/her why
        /// if run is in progress, send the race data
        /// </summary>
        /// <returns></returns>
        private async Task SendCurrentState()
        {
            //Send status
            var toSend = new Message<AdminCommands>
            {
                Command = AdminCommands.Status,
                Data = RaceManager.Instance.CurrentState
            };
            await SendMessageAsync(_admin, toSend);

            if (RaceManager.Instance.CurrentState == RaceManager.State.InProgress)
            {
                //Send basic race data
                var message = new Message<AdminCommands>
                {
                    Command = AdminCommands.RunStart,
                    Data = new RunStartDTO
                    {
                        StartTime = RaceManager.Instance.TimeMeter.StartTime,
                        CurrentTime = RaceManager.Instance.TimeMeter.ApproximatedCurrentTime,
                        Runners = RaceManager.Instance.Runners
                    }
                };
                await SendMessageAsync(_admin, message);
            }
        }
    }
}