using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeMeasurement_Backend.Handlers
{
    public class Message<T> where T : Enum
    {
        /// <summary>
        /// Command used to identify purpose of Message 
        /// </summary>
        public T Command { get; set; }

        /// <summary>
        /// The "Arguments" which come with the command
        /// </summary>
        public object Data { get; set; }
    }
}
