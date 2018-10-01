using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TimeMeasurement_Backend.Handlers
{
    public abstract class Handler<T> where T : Enum
    {
        /// <summary>
        /// Starts listening to Websocket and passes received Messages to HandleMessage
        /// </summary>
        /// <param name="ws">The websocket to listen to</param>
        /// <returns></returns>
        protected async Task ListenAsync(WebSocket ws)
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
                    return;
                }
                //Convert reveived data to JSON string, then to Message
                var received = JsonConvert.DeserializeObject<Message<T>>(Encoding.UTF8.GetString(receiveBuffer));
                HandleMessage(ws, received);
            }
        }

        /// <summary>
        /// Sends a message to a websocket client
        /// </summary>
        /// <param name="receiver">The target websocket</param>
        /// <param name="toSend">The message</param>
        /// <returns></returns>
        protected async Task SendMessageAsync(WebSocket receiver, Message<T> toSend)
        {
            if (receiver == null)
            {
                return;
            }

            //Convert Message to JSON, then to byte[]
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toSend));
            await receiver.SendAsync(
                new ArraySegment<byte>(data, 0, data.Count(b => b != 0)), //Arraysegment with length of not 0 bytes
                WebSocketMessageType.Text,
                true, //Message is not split
                CancellationToken.None
            );
        }

        /// <summary>
        /// Used to process received messages
        /// </summary>
        /// <param name="sender">The sender of the message</param>
        /// <param name="received">The message</param>
        protected abstract void HandleMessage(WebSocket sender, Message<T> received);
    }
}
