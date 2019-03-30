using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteelSeries.GameSense
{
    /// <summary>
    /// GameSense surface API.
    /// </summary>
    public static class GameSense
    {
        private static GameSenseClient m_client;
        private static string m_gameName;

        public static GameSenseClient Client
        {
            get { return m_client; }
        }

        public static string GameName
        {
            get { return m_gameName; }
        }

        public static bool IsInitialized
        {
            get { return m_client != null; }
        }

        public static void Initialize(string gameName, string displayGameName, string developer, IconColor iconColor)
        {
            Release();
            m_gameName = gameName;
            m_client = new GameSenseClient();
            m_client.RegisterGame(gameName, displayGameName, developer, iconColor);
            m_client.EnableHeartbeat(gameName, true);
        }

        public static void Release()
        {
            if (m_client != null)
            {
                m_client.Dispose();
                m_client = null;
                m_gameName = null;
            }
        }

        /// <summary>
        /// Registers event.
        /// </summary>
        public static void RegisterEvent(string eventName, int minValue, int maxValue, EventIconId iconId, bool valueOptional = false)
        {
            m_client.RegisterEvent(m_gameName, eventName, minValue, maxValue, iconId, valueOptional);
        }

        /// <summary>
        /// Binds event.
        /// </summary>
        public static void BindEvent(string eventName, int minValue, int maxValue, EventIconId iconId, string[] handlers)
        {
            if (handlers.Length == 0)
            {
                m_client.RegisterEvent(m_gameName, eventName, minValue, maxValue, iconId);
            }
            else
            {
                m_client.BindEvent(m_gameName, eventName, minValue, maxValue, iconId, handlers);
            }
        }

        /// <summary>
        /// Sends event.
        /// </summary>
        /// <param name="eventName">Event name.</param>
        /// <param name="value">Event value.</param>
        public static void SendEvent(string eventName, int value)
        {
            m_client?.SendEvent(m_gameName, eventName, value);
        }
    }
}
