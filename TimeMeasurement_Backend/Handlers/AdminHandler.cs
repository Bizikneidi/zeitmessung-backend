using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TimeMeasurement_Backend.Handlers
{
    /// <summary>
    /// Handles websocket connection with an admin
    /// </summary>
    public class AdminHandler : Handler<AdminHandler.Command>
    {
        public enum Command
        {
            Start //Station should start measuring time
        }

        /// <summary>
        /// The weboscket corresponding to the admin
        /// </summary>
        private WebSocket _admin;

        public static AdminHandler Instance { get; } = new AdminHandler();

        /// <summary>
        /// Connect with admin and listen for its messages
        /// </summary>
        /// <param name="admin">The Websocket corrresponding to the admin</param>
        /// <returns></returns>
        public async Task SetAdmin(WebSocket admin)
        {
            //Someone has already connected as admin
            if (_admin != null)
            {
                return;
            }

            _admin = admin;
            await ListenAsync(_admin);
        }

        protected override void HandleMessage(WebSocket sender, Message<Command> received)
        {
            if (received.Command == Command.Start)
            {
                //Tell station to start measuring time
                StationHandler.Instance.SendStartSignal();
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}