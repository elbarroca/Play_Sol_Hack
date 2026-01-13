using UnityEngine;
using PlaceholderHack.Core;
using PlaceholderHack.Games.Sumo;

namespace PlaceholderHack.Networking
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance;

        [Header("Mode Selection")]
        public bool UseMockMode = true; // ✅ CHECK THIS TO DEVELOP OFFLINE

        [Header("References")]
        public MagicBlockClient RealClient;
        public MockGameStateProvider MockClient;
        public NetworkSumo VisualsP1;
        public NetworkSumo VisualsP2;

        private IGameStateProvider _activeProvider;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // 1. Select Provider
            if (UseMockMode)
            {
                _activeProvider = MockClient;
                RealClient.enabled = false;
                MockClient.enabled = true;
                Debug.Log("⚠️ Running in MOCK MODE (Offline)");
            }
            else
            {
                _activeProvider = RealClient;
                MockClient.enabled = false;
                RealClient.enabled = true;
                Debug.Log("🌍 Running in REAL MODE (Blockchain)");
            }
        }

        void Update()
        {
            if (_activeProvider == null || _activeProvider.CurrentState == null) return;

            var state = _activeProvider.CurrentState;

            // 2. Drive Visuals (Read)
            if (VisualsP1 != null)
                VisualsP1.OnServerUpdate(state.P1Coords[0], state.P1Coords[1]);

            if (VisualsP2 != null)
                VisualsP2.OnServerUpdate(state.P2Coords[0], state.P2Coords[1]);
        }

        // 3. Handle Input (Write)
        public void SendMove(sbyte x, sbyte y)
        {
            _activeProvider?.SendInput(x, y);
        }
    }
}