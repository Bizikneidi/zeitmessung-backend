using System;

namespace TimeMeasurement_Backend.Networking.Messaging
{
    /// <summary>
    /// Used to send Commands and Data between websockets, (is converted to JSON)
    /// </summary>
    /// <typeparam name="TCommands">The available commands</typeparam>
    public class Message<TCommands> where TCommands : Enum
    {
        /// <summary>
        /// Command used to identify purpose of Message
        /// </summary>
        public TCommands Command { get; set; }

        /// <summary>
        /// The Data which comes with the command
        /// </summary>
        public object Data { get; set; }
    }
}