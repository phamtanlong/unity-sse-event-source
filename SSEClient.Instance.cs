using UnityEngine;

namespace UnitySseEventSource {
    public partial class SSEClient {
        private static SSEClient _instance;

        public static SSEClient instance {
            get {
                if (_instance == null) {
                    var go = new GameObject("SSEClient");
                    _instance = go.AddComponent<SSEClient>();
                    DontDestroyOnLoad(go);
                }

                return _instance;
            }
        }

        public static void DestroyInstance() {
            if (_instance != null) {
                Destroy(_instance.gameObject);
            }
        }
    }
}