using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Logic;
using TimeMeasurement_Backend.Networking.MessageData;

namespace TimeMeasurement_Backend.Networking.Handlers
{
    /// <summary>
    /// Handles a websocket connection with an admin
    /// </summary>
    public class AdminHandler : Handler<AdminCommands>
    {
        /// The weboscket corresponding to the admin
        private WebSocket _admin;

        public AdminHandler()
        {
            RaceManager.Instance.StateChanged += OnRaceManagerStateChanged;
            TimeMeter.Instance.OnMeasurement += SendMeasuredStop;
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
            SendAvailableRaces();
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
                    if (received.Data is long raceId) //Check if received data is valid
                    {
                        RaceManager.Instance.RequestStart((int)raceId);
                    }

                    break;
                //Admin has created a race
                case AdminCommands.CreateRace:
                    var race = ((JObject)received.Data).ToObject<Race>();
                    RaceManager.Instance.CreateRace(race);
                    break;
                //Admin has assigned a time to a runner
                case AdminCommands.AssignTime:
                    var assignment = ((JObject)received.Data).ToObject<AssignmentDTO>();
                    //Try to assign the time
                    if (!ParticipantManager.Instance.TryAssignTimeToRunner(assignment.Starter, assignment.Time))
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

            if (prev != RaceManager.State.InProgress)
            {
                return;
            }

            SendRaceEnd();
            SendAvailableRaces();
        }

        /// <summary>
        /// Send all races the admin could start at the moment
        /// </summary>
        private void SendAvailableRaces()
        {
            var message = new Message<AdminCommands>
            {
                Command = AdminCommands.AvailableRaces,
                Data = RaceManager.Instance.StartableRaces
            };
            SendMessage(_admin, message);
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
                    StartTime = TimeMeter.Instance.StartTime,
                    CurrentTime = TimeMeter.Instance.ApproximatedCurrentTime,
                    Participants = ParticipantManager.Instance.CurrentParticipants
                }
            };
            SendMessage(_admin, message);

            //Also send all not yet assigned times
            foreach (long measurement in ParticipantManager.Instance.UnassignedMeasurements)
            {
                SendMeasuredStop(measurement);
            }
        }
    }
}