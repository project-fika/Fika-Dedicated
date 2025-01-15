using BepInEx.Logging;
using EFT.UI;
using Fika.Core.Networking.Http;
using Fika.Headless;
using Newtonsoft.Json.Linq;
using SPT.Common.Http;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking.Match;
using WebSocketSharp;

namespace Fika.Core.Networking
{
    public class HeadlessRaidWebSocketClient : MonoBehaviour
    {
        private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("Fika.HeadlessWebSocket");

        public string Host { get; set; }
        public string Url { get; set; }
        public string SessionId { get; set; }
        public bool Connected
        {
            get
            {
                return _webSocket.ReadyState == WebSocketState.Open;
            }
        }

        private WebSocket _webSocket;
        // Add a queue for incoming messages, so they can be brought to the main thread in a nice way.
        private ConcurrentQueue<StartHeadlessRequest> messagesQueue = new();

        public HeadlessRaidWebSocketClient()
        {
            Host = RequestHandler.Host.Replace("http", "ws");
            SessionId = RequestHandler.SessionId;
            Url = $"{Host}/fika/headlessraidservice/";

            _webSocket = new WebSocket(Url)
            {
                WaitTime = TimeSpan.FromMinutes(1),
                EmitOnPing = true
            };

            _webSocket.SetCredentials(SessionId, "", true);

            _webSocket.OnOpen += WebSocket_OnOpen;
            _webSocket.OnMessage += WebSocket_OnMessage;
            _webSocket.OnError += WebSocket_OnError;
            _webSocket.OnClose += WebSocket_OnClose;
        }

        public void Connect()
        {
            logger.LogInfo($"WS Connect()");
            logger.LogInfo($"Attempting to connect to {Url}...");
            _webSocket.Connect();
        }

        public void Update()
        {
            while (messagesQueue.TryDequeue(out StartHeadlessRequest request))
            {
                FikaHeadlessPlugin.Instance.OnFikaStartRaid(request);
            }
        }

        public void Close()
        {
            _webSocket.Close();
        }

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            logger.LogInfo("Connected to FikaDedicatedRaidWebSocket as server");
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            JObject jsonObject = JObject.Parse(e.Data);

            if (!jsonObject.ContainsKey("type"))
            {
                return;
            }

            string type = jsonObject["type"].Value<string>();

            switch (type)
            {
                case "fikaHeadlessStartRaid":
                    StartHeadlessRequest request = jsonObject.ToObject<StartHeadlessRequest>();
                    messagesQueue.Enqueue(request);
                    break;
                case "fikaHeadlessKeepAlive":
                    _webSocket.Send("keepalive");
                    break;
            }
        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            logger.LogInfo($"FikaDedicatedRaidWebSocket error: {e.Message}");
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs closeEventArgs)
        {
            if (!closeEventArgs.WasClean)
            {
                Task.Run(RetryConnect);
            }
        }

        private async void RetryConnect()
        {
            logger.LogWarning($"Websocket connection lost, retrying...");

            await Task.Delay(5000);
            Connect();
        }
    }
}
