using UnityEngine;

namespace UnitySseEventSource {
    public class LogSseExampleDownloadHandler : SseDownloadHandlerBase {
        public LogSseExampleDownloadHandler() : base(new byte[1024]) { }

        protected override void OnNewLineReceived(string line) {
            Debug.Log(line);
        }
    }
}