using System;
using System.Buffers;

namespace UnitySseEventSource {
    public class LogSseWithPooledBufferExampleDownloadHandler : SseDownloadHandlerBase {
        private byte[] _pooledBuffer;
        public Action<string> MessageReceived;

        public static LogSseWithPooledBufferExampleDownloadHandler Create() {
            var buffer = ArrayPool<byte>.Shared.Rent(1024);
            return new LogSseWithPooledBufferExampleDownloadHandler(buffer);
        }

        private LogSseWithPooledBufferExampleDownloadHandler(byte[] buffer) : base(buffer) {
            _pooledBuffer = buffer;
        }

        protected override void OnNewLineReceived(string line) {
            MessageReceived?.Invoke(line);
        }

        public override void Dispose() {
            base.Dispose();
            ArrayPool<byte>.Shared.Return(_pooledBuffer);
        }
    }
}