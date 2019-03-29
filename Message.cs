using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteelSeries.GameSense
{
    struct Message
    {
        public string Url;

        // Serialized data
        public string SerializedData;

        // Non-serialized data for SendEvent
        public string GameName;
        public string EventName;
        public int EventData;

        public Message(string url, string data)
            : this()
        {
            Url = url;
            SerializedData = data;
        }

        public Message(string url, string gameName, string eventName, int eventData)
        {
            Url = url;
            SerializedData = null;
            GameName = gameName;
            EventName = eventName;
            EventData = eventData;
        }
    }
}
