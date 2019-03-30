using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SteelSeries.GameSense
{
    /// <summary>
    /// GameSense client, sends messages to server.
    /// </summary>
    public class GameSenseClient : IDisposable
    {
        public string BaseUrl;
        public string EventUrl;

        LocklessQueue<Message> m_queue;
        GameSenseWorker m_worker;

        public GameSenseWorker Worker
        {
            get { return m_worker; }
        }

        public GameSenseClient()
        {
            BaseUrl = GameSenseProps.GetServerUrl();
            if (BaseUrl != null)
            {
                EventUrl = BaseUrl + "/game_event";
                m_queue = new LocklessQueue<Message>(256);
                m_worker = new GameSenseWorker(m_queue, BaseUrl);
            }
        }

        public void Dispose()
        {
            if (m_worker != null)
            {
                m_worker.Dispose();
                m_worker = null;
            }
        }

        public void EnableHeartbeat(string gameName, bool enable)
        {
            if (m_worker != null)
            {
                m_worker.EnableHeartbeat(gameName, enable);
            }
        }

        public void RegisterGame(string gameName, string displayName, string developer, IconColor iconColor)
        {
            string msg = $@"
            {{
                ""game"": ""{gameName}"",
                ""game_display_name"": ""{displayName}"",
                ""developer"": ""{developer}"",
                ""icon_color_id"": {(int)iconColor}
            }}";
            SendCustomMessage("game_metadata", msg);
        }

        public void RemoveGame(string gameName)
        {
            string msg = $@"
            {{
                ""game"": ""{gameName}""
            }}";
            SendCustomMessage("remove_game", msg);
        }

        public void RegisterEvent(string gameName, string eventName, int minValue, int maxValue, EventIconId iconId, bool valueOptional = false)
        {
            string valueOptionalText = valueOptional ? "true" : "false";
            string msg = $@"
            {{
                ""game"": ""{gameName}"",
                ""event"": ""{eventName}"",
                ""min_value"": {minValue},
                ""max_value"": {maxValue},
                ""icon_id"": {(int)iconId},
                ""value_optional"": {valueOptionalText}
            }}";
            SendCustomMessage("register_game_event", msg);
        }

        public void BindEvent(string gameName, string eventName, int minValue, int maxValue, EventIconId iconId, string[] handlers)
        {
            string msg = $@"
            {{
                ""game"": ""{gameName}"",
                ""event"": ""{eventName}"",
                ""min_value"": {minValue},
                ""max_value"": {maxValue},
                ""icon_id"": {(int)iconId},
                ""handlers"": [
                    {string.Join(", ", handlers)}
                ]
            }}";
            SendCustomMessage("bind_game_event", msg);
        }

        public void RemoveEvent(string gameName, string eventName)
        {
            string msg = $@"
            {{
                ""game"": ""{gameName}"",
                ""event"": ""{eventName}""
            }}";
            SendCustomMessage("remove_game_event", msg);
        }

        public void SendEvent(string gameName, string eventName, int value)
        {
            if (m_worker != null && m_worker.IsRunning)
            {
                m_queue.Enqueue(new Message(EventUrl, gameName, eventName, value));
            }
        }

        public void SendCustomMessage(string endpoint, string message)
        {
            if (m_worker != null && m_worker.IsRunning)
            {
                m_queue.Enqueue(new Message($"{BaseUrl}/{endpoint}", message));
            }
        }
    }
}