using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TimeMeasurement_Backend.Handlers
{
    /// <summary>
    /// Handles websocket connections with multiple viewers
    /// </summary>
    public class ViewerHandler : Handler<ViewerHandler.Commands>
    {
        public enum Commands
        {
            //TODO
        }

        public static ViewerHandler Instance { get; } = new ViewerHandler();

        /// <summary>
        /// Connect with viewer and listen for its messages
        /// </summary>
        /// <param name="viewer">The Websocket corrresponding to a viewer</param>
        /// <returns></returns>
        public async Task AddViewer(WebSocket viewer)
        {
            //Simply listen for messages
            await ListenAsync(viewer);
        }

        protected override void HandleMessage(WebSocket sender, Message<Commands> received)
        {
            //TODO
        }
    }
}