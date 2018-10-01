using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TimeMeasurement_Backend.Handlers
{
    public class StationHandler
    {
        public static StationHandler Instance { get; } = new StationHandler();

        private WebSocket _websocket;

        /// <summary>
        /// Send a start signal to the station to tell it to start measuring the time
        /// </summary>
        public void SendStartSignal()
        {
            //Station has not yet connected
            if (_websocket == null)
            {
                return;
            }

            var toSend = new Message
            {
                Cmd = Message.Command.Start,
                Data = null //No data
            };

            //Convert Message to JSON, then to byte[]
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toSend));
            _websocket.SendAsync(
                new ArraySegment<byte>(data, 0, data.Count(b => b != 0)), //Arraysegment with length of not 0 bytes
                WebSocketMessageType.Text,
                true, //Message is not split
                CancellationToken.None
            );
        }

        /// <summary>
        /// Connect with station and listen for its messages
        /// </summary>
        /// <param name="ws">The Websocket corrresponding to the station</param>
        /// <returns></returns>
        public async Task SetStation(WebSocket ws)
        {
            _websocket = ws;

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
                    _websocket = null;
                    return;
                }
                //Convert reveived data to JSON string, then to Message
                var received = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(receiveBuffer));
                HandleMessage(received);
            }
        }

        /// <summary>
        /// Process received messages
        /// </summary>
        /// <param name="message"></param>
        private void HandleMessage(Message message)
        {
            switch (message.Cmd)
            {
                case Message.Command.StartTime:
                    //Station has sent start time
                    break;
                case Message.Command.EndTime:
                    //Station has sent end time
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public class Message
        {
            public enum Command
            {
                Start, //Station should start measuring time
                StartTime, //Message contains start time
                EndTime //Message contains end time
            }

            /// <summary>
            /// Command used to identify purpose of Message 
            /// </summary>
            public Command Cmd { get; set; }

            /// <summary>
            /// The "Arguments" which come with the command
            /// </summary>
            public object Data { get; set; }
        }
    }
}
