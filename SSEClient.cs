using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace UnitySseEventSource {
    public class SSEMessage {
        public string Data { get; set; }
        public int Retry { get; set; }
        public string Id { get; set; }
        public string Event { get; set; }

        public override string ToString() {
            return $"Event: {Event}, Id: {Id}, Retry: {Retry} Data: {Data}";
        }
    }

    public partial class SSEClient : MonoBehaviour {
        public bool logLine;
        public Action<SSEMessage> MessageReceived;
        public Action Disconnected;
        public Action BeforeConnected;

        private UnityWebRequest request;
        private LogSseWithPooledBufferExampleDownloadHandler downloadHandler;
        private void NextRetry()
        {
            switch (retryCount)
            {
                case 0: retry = 3; break;
                case 1: retry = 6; break;
                case 2: retry = 12; break;
                case 3: retry = 24; break;
                default: retry = 60; break;
            }
        }
        private int retryCount;
        private float retry = 3; // seconds
        private string lastEventId = string.Empty;

        public void Connect(string url, string token) {
            var headers = new Dictionary<string, string> {
                { "Authorization", "Bearer " + token }
            };
            Connect(url, headers);
        }

        public void Connect(string url, Dictionary<string, string> headers) {
            StartCoroutine(ConnectAsync(url, headers));
        }

        private IEnumerator ConnectAsync(string url, Dictionary<string, string> headers) {
            while (true) {
                downloadHandler = LogSseWithPooledBufferExampleDownloadHandler.Create();
                downloadHandler.MessageReceived += OnMessageReceived;

                request = UnityWebRequest.Get(url);
                foreach (var p in headers) {
                    request.SetRequestHeader(p.Key, p.Value);
                }

                if (!string.IsNullOrEmpty(lastEventId)) {
                    request.SetRequestHeader("Last-Event-Id", lastEventId);
                }

                request.downloadHandler = downloadHandler;

                BeforeConnected?.Invoke();
                yield return request.SendWebRequest();

                if (request.result is UnityWebRequest.Result.ConnectionError
                    or UnityWebRequest.Result.ProtocolError) {
                    Disconnected?.Invoke();
                    Debug.Log($"[SSE] Connection error (retry after {retry}s)\n" + request.error);
                    retryCount++;
                    NextRetry();
                    yield return new WaitForSeconds(retry);
                    Debug.Log($"[SSE] Retry now");
                }
                else {
                    yield return null;
                }

                Dispose();
            }
        }

        private SSEMessage message = new();

        public void OnMessageReceived(SSEMessage msg) {
            Debug.Log("[SSE] Received\n" + msg);
            MessageReceived?.Invoke(msg);
        }

        private void OnMessageReceived(string line) {
            if (line.StartsWith("event:")) {
                message = new SSEMessage();
                var eventField = line.Substring(6).Trim();
                if (logLine) Debug.Log("[SSE] Event: " + eventField);
                message.Event = eventField;
            }
            else if (line.StartsWith("id:")) {
                var idField = line.Substring(3).Trim();
                lastEventId = idField;
                if (logLine) Debug.Log("[SSE] ID: " + idField);
                message.Id = idField;
            }
            else if (line.StartsWith("retry:")) {
                var retryField = line.Substring(6).Trim();
                if (logLine) Debug.Log("[SSE] Retry: " + retryField);
                if (int.TryParse(retryField, out var retryInt)) {
                    retry = retryInt / 1000f;
                    message.Retry = retryInt;
                }
            }
            else if (line.StartsWith("data:")) {
                var dataField = line.Substring(5).Trim();
                if (logLine) Debug.Log("[SSE] Data: " + dataField);
                message.Data = dataField;
                OnMessageReceived(message);
            }
        }

        private void Dispose() {
            if (downloadHandler != null) {
                downloadHandler.MessageReceived -= OnMessageReceived;
                downloadHandler.Dispose();
            }

            request?.Dispose();
        }

        private void OnDestroy() {
            Dispose();
        }
    }
}