using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace UnitySseEventSource {
    public abstract class SseDownloadHandlerBase : DownloadHandlerScript {
        private readonly List<byte> _currentLine = new();

        protected SseDownloadHandlerBase(byte[] buffer) : base(buffer) { }

        protected abstract void OnNewLineReceived(string line);

        protected override bool ReceiveData(byte[] bytes, int dataLength) {
            for (var i = 0; i < dataLength; i++) {
                var b = bytes[i];
                if (b == '\n') {
                    OnNewLineReceived(Encoding.UTF8.GetString(_currentLine.ToArray(), 0, _currentLine.Count));
                    _currentLine.Clear();
                }
                else {
                    _currentLine.Add(b);
                }
            }

            return true;
        }

        protected override void CompleteContent() {
            if (_currentLine.Count > 0)
                OnNewLineReceived(Encoding.UTF8.GetString(_currentLine.ToArray(), 0, _currentLine.Count));
        }
    }
}