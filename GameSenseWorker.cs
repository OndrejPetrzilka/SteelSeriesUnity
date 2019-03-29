using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SteelSeries.GameSense
{
    /// <summary>
    /// Worker running on separate thread, reads message queue and sends messages through HttpClient.
    /// </summary>
    public class GameSenseWorker : IDisposable
    {
        static MediaTypeHeaderValue m_mediaType = new MediaTypeHeaderValue("application/json");

        private LocklessQueue<Message> m_queue;
        private Thread m_thread;
        private bool m_isRunning;
        private string m_heartbeatUrl;
        private byte[] m_payload = new byte[1024];
        private char[] m_event = new char[256];
        private List<string> m_heartbeats = new List<string>(1);
        private string[] m_heartbeatMessages = new string[0];

        public TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(1);
        public TimeSpan SleepDuration = TimeSpan.FromMilliseconds(10);
        public TimeSpan ServerDownInterval = TimeSpan.FromSeconds(5);
        public bool Logging = true;

        public bool IsRunning
        {
            get { return m_isRunning; }
        }

        internal GameSenseWorker(LocklessQueue<Message> queue, string baseUrl)
        {
            if (Logging)
            {
                Debug.Log("SteelSeries engine started on: " + baseUrl);
            }

            m_heartbeatUrl = baseUrl + "/game_heartbeat";
            m_queue = queue;
            m_isRunning = true;
            m_thread = new Thread(WorkerThread);
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        public void Dispose()
        {
            if (m_thread != null)
            {
                m_isRunning = false;
                m_thread = null;
            }
        }

        public void EnableHeartbeat(string gameName, bool enable)
        {
            if (enable && !m_heartbeats.Contains(gameName))
            {
                m_heartbeats.Add(gameName);
                RebuildHeartbeats();
            }
            else if (!enable && m_heartbeats.Remove(gameName))
            {
                RebuildHeartbeats();
            }
        }

        private void RebuildHeartbeats()
        {
            var result = new string[m_heartbeats.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = $@"{{ ""game"": ""{m_heartbeats[i]}"" }}";
            }

            // Reference assignment is atomic
            m_heartbeatMessages = result;
        }

        void WorkerThread()
        {
            using (HttpClient client = new HttpClient())
            {
                var timer = System.Diagnostics.Stopwatch.StartNew();

                bool isRunning = true;
                while (isRunning)
                {
                    // Ensure all messages in queue gets processes when Dispose is called
                    isRunning = m_isRunning;

                    // Process messages
                    while (m_queue.TryDequeue(out Message msg))
                    {
                        if (msg.SerializedData != null)
                        {
                            // Sending critical message
                            int payloadLength = InitPayload(msg.SerializedData);
                            while (!SendAndValidate(client, msg.Url, m_payload, payloadLength) && m_isRunning)
                            {
                                Thread.Sleep(ServerDownInterval);
                            }
                        }
                        else
                        {
                            // Sending event, fire-and-forget
                            int payloadLength = InitPayload(msg.GameName, msg.EventName, msg.EventData);
                            Send(client, msg.Url, m_payload, payloadLength);
                            if (Logging)
                            {
                                Debug.Log("Sent event: " + Encoding.UTF8.GetString(m_payload, 0, payloadLength));
                            }
                        }
                        timer.Restart();
                    }

                    // Send heartbeats
                    if (timer.Elapsed > HeartbeatInterval)
                    {
                        // Reference assignment is atomic
                        var heartbeats = m_heartbeatMessages;
                        foreach (var item in heartbeats)
                        {
                            // Sending heartbeat, fire-and-forget
                            int payloadLength = InitPayload(item);
                            Send(client, m_heartbeatUrl, m_payload, payloadLength);
                            if (Logging)
                            {
                                //Debug.Log("Sent Heartbeat");
                            }
                        }
                        timer.Restart();
                    }

                    // Sleep
                    Thread.Sleep(SleepDuration);
                }
                if (Logging)
                {
                    Debug.Log("SteelSeries engine stopped");
                }
            }
        }

        private Task<HttpResponseMessage> Send(HttpClient client, string url, byte[] payload, int payloadLength)
        {
            // Could be optimized further by using TcpClient and writing HTTP requests manually
            var content = new ByteArrayContent(payload, 0, payloadLength);
            content.Headers.ContentType = m_mediaType;
            return client.PostAsync(url, content);
        }

        private bool SendAndValidate(HttpClient client, string url, byte[] payload, int payloadLength)
        {
            try
            {
                var response = Send(client, url, payload, payloadLength).Result;
                if (!response.IsSuccessStatusCode)
                {
                    var responseText = response.Content.ReadAsStringAsync().Result;
                    var msgText = Encoding.UTF8.GetString(payload, 0, payloadLength);
                    Debug.LogError("Error processing message: " + responseText + Environment.NewLine + "Url: " + url + Environment.NewLine + msgText);
                }
                else if (Logging)
                {
                    Debug.Log("Server response: " + response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception e)
            {
                var webException = e.InnerException?.InnerException as WebException;
                if (webException != null && webException.Status == WebExceptionStatus.ConnectFailure)
                {
                    return false;
                }
                else
                {
                    Debug.LogException(e);
                }
            }
            return true;
        }

        private int InitPayload(string gameName, string eventName, int eventData)
        {
            // Fast path for events
            int count = 0;
            Append(m_event, ref count, @"{ ""game"": """);
            Append(m_event, ref count, gameName);
            Append(m_event, ref count, @""", ""event"": """);
            Append(m_event, ref count, eventName);
            Append(m_event, ref count, @""", ""data"": { ""value"": ");
            Append(m_event, ref count, eventData.ToString());
            Append(m_event, ref count, @" } }");

            EnsurePayloadLength(count);
            return Encoding.UTF8.GetBytes(m_event, 0, count, m_payload, 0);
        }

        private int InitPayload(string str)
        {
            EnsurePayloadLength(str.Length);
            return Encoding.UTF8.GetBytes(str, 0, str.Length, m_payload, 0);
        }

        private void EnsurePayloadLength(int requiredCharacterCount)
        {
            int maxLength = Encoding.UTF8.GetMaxByteCount(requiredCharacterCount);
            if (m_payload.Length < maxLength)
            {
                m_payload = new byte[Math.Max(maxLength, m_payload.Length * 2)];
            }
        }

        static void Append(char[] array, ref int index, string value)
        {
            for (int i = 0; i < value.Length; i++, index++)
            {
                array[index] = value[i];
            }
        }
    }
}