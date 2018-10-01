using System.Net.WebSockets;
using System.Threading.Tasks;
using TimeMeasurement_Backend.Handlers.Messaging;

namespace TimeMeasurement_Backend.Handlers
{
    /// <summary>
    /// Handles websocket connections with multiple viewers
    /// </summary>
    public class ViewerHandler : Handler<ViewerCommands>
    {
        public static ViewerHandler Instance { get; } = new ViewerHandler();

        /// <summary>
        /// Connect with viewer and listen for its messages
        /// </summary>
        /// <param name="viewer">The Websocket corrresponding to a viewer</param>
        /// <returns></returns>
        public async Task AddViewerAsync(WebSocket viewer)
        {
            //Simply listen for messages
            await ListenAsync(viewer);
        }

        protected override void HandleMessage(WebSocket sender, Message<ViewerCommands> received)
        {
            //TODO
        }
    }
}