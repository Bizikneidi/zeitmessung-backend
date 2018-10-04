using System.Net.WebSockets;
using System.Threading.Tasks;
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

        public AdminHandler() => TimeMeter.Instance.StateChanged += OnTimeMeterStateChanged;

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
            if (received.Command != AdminCommands.Start)
            {
                return;
            }

            //Admin has pressed start
            //Tell time meter to start measuring
            TimeMeter.Instance.RequestMeasurement();
        }

        protected override void OnDisconnect(WebSocket disconnected)
        {
            _admin = null;
        }

        private void OnTimeMeterStateChanged(TimeMeter.State prev, TimeMeter.State current)
        {
            //Notify admin
            Task.Run(async () => await SendCurrentState());
        }

        /// <summary>
        /// Tell the admin wether he is allowed to start a measurement / if not tell him/her why
        /// </summary>
        /// <returns></returns>
        private async Task SendCurrentState()
        {
            var toSend = new Message<AdminCommands>
            {
                Command = AdminCommands.Status,
                Data = TimeMeter.Instance.CurrentState
            };
            await SendMessageAsync(_admin, toSend);
        }
    }
}