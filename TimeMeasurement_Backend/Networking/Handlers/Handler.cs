using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TimeMeasurement_Backend.Networking.MessageData;

namespace TimeMeasurement_Backend.Networking.Handlers
{
    /// <summary>
    /// Handles connections with websockets over Messages
    /// </summary>
    /// <typeparam name="TCommands">The available Commands for the Messages</typeparam>
    public abstract class Handler<TCommands> where TCommands : Enum
    {
        /// <summary>
        /// Broadcasts a message to all receivers
        /// </summary>
        /// <param name="receivers">The target websockets</param>
        /// <param name="toBroadcast">The message</param>
        protected void BroadcastMessage(IEnumerable<WebSocket> receivers, Message<TCommands> toBroadcast)
        {
            //Convert Message to JSON, then to byte[]
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toBroadcast));
            //Arraysegment with length of not 0 bytes
            var segment = new ArraySegment<byte>(data, 0, data.Count(b => b != 0));
            //Send to all
            foreach (var receiver in receivers)
            {
                if (receiver == null)
                {
                    return;
                }

                receiver.SendAsync(
                    segment,
                    WebSocketMessageType.Text,
                    true, //Message is not split
                    CancellationToken.None
                );
            }
        }

        /// <summary>
        /// Used to process received messages
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="received">The message</param>
        protected abstract void HandleMessage(WebSocket sender, Message<TCommands> received);

        /// <summary>
        /// Starts listening to Websocket and passes received Messages to HandleMessage
        /// </summary>
        /// <param name="ws">The websocket to listen to</param>
        /// <returns></returns>
        protected async Task ListenAsync(WebSocket ws)
        {
            //ws must not be null
            if (ws == null)
            {
                return;
            }

            try
            {
                while (true)
                {
                    //wait for input and read data into buffer
                    var receiveBuffer = new byte[4096];
                    var rs = await ws.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                    //Connection is being closed
                    if (rs.CloseStatus.HasValue)
                    {
                        //Close and exit
                        await ws.CloseAsync(rs.CloseStatus.Value, rs.CloseStatusDescription, CancellationToken.None);
                        OnDisconnect(ws);
                        return;
                    }

                    //Convert reveived data to JSON string, then to Message
                    var received = JsonConvert.DeserializeObject<Message<TCommands>>(Encoding.UTF8.GetString(receiveBuffer));
                    HandleMessage(ws, received);
                }
            }
            catch (Exception) //Client has force closed the connection
            {
                OnDisconnect(ws);
            }
        }

        /// <summary>
        /// Gets called, whenever a websocket disconnects
        /// </summary>
        /// <param name="disconnected">The ws which lost connection</param>
        protected abstract void OnDisconnect(WebSocket disconnected);

        /// <summary>
        /// Sends a message to a websocket client
        /// </summary>
        /// <param name="receiver">The target websocket</param>
        /// <param name="toSend">The message</param>
        /// <returns></returns>
        protected void SendMessage(WebSocket receiver, Message<TCommands> toSend)
        {
            //receiver must not be null
            if (receiver == null)
            {
                return;
            }

            //Convert Message to JSON, then to byte[]
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toSend));

            receiver.SendAsync(
                new ArraySegment<byte>(data, 0, data.Count(b => b != 0)), //Arraysegment with length of not 0 bytes
                WebSocketMessageType.Text,
                true, //Message is not split
                CancellationToken.None
            );
        }
    }
}