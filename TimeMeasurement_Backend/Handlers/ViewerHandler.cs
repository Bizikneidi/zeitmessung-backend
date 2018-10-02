using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TimeMeasurement_Backend.Handlers.Messaging;

namespace TimeMeasurement_Backend.Handlers
{
    /// <summary>
    /// Handles websocket connections with multiple viewers
    /// </summary>
    public class ViewerHandler : Handler<ViewerCommands>
    {
        /// <summary>
        /// All connected Viewers
        /// </summary>
        private readonly List<WebSocket> _viewers;

        public static ViewerHandler Instance { get; } = new ViewerHandler();

        private ViewerHandler() => _viewers = new List<WebSocket>();

        /// <summary>
        /// Connect with viewer and listen for its messages
        /// </summary>
        /// <param name="viewer">The Websocket corrresponding to a viewer</param>
        /// <returns></returns>
        public async Task AddViewerAsync(WebSocket viewer)
        {
            _viewers.Add(viewer);
            //Simply listen for messages
            await ListenAsync(viewer);
        }

        /// <summary>
        /// Tells all viewers that a run has ended at the following time
        /// </summary>
        public void BroadcastRunEnd()
        {
            var message = new Message<ViewerCommands>
            {
                Command = ViewerCommands.RunEnd,
                Data = null
            };
            Broadcast(message);
        }

        /// <summary>
        /// Tells all viewers that a run has startet at the following time
        /// </summary>
        public void BroadcastRunStart()
        {
            var message = new Message<ViewerCommands>
            {
                Command = ViewerCommands.RunStart,
                Data = null
            };
            Broadcast(message);
        }

        protected override void HandleMessage(WebSocket sender, Message<ViewerCommands> received)
        {
            //TODO
        }

        protected override void OnDisconnect(WebSocket disconnected)
        {
            //Remove from active viewers
            _viewers.Remove(disconnected);
        }

        /// <summary>
        /// Broadcasts a message to all connected viewers
        /// </summary>
        /// <param name="toBroadcast">the message to send out</param>
        private void Broadcast(Message<ViewerCommands> toBroadcast)
        {
            //Convert Message to JSON, then to byte[]
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toBroadcast));
            //Arraysegment with length of not 0 bytes
            var segment = new ArraySegment<byte>(data, 0, data.Count(b => b != 0));
            //Parallel due to await
            Parallel.ForEach(_viewers, async receiver =>
            {
                await receiver.SendAsync(
                    segment,
                    WebSocketMessageType.Text,
                    true, //Message is not split
                    CancellationToken.None
                );
            });
        }
    }
}