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
        private bool _hasTriggeredGameOver = false;

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

            // ---------------------------------------------------------
            // 3. WIN CONDITION CHECK
            // ---------------------------------------------------------
            if (state.Status == PlaceholderHack.Core.GameStatus.Finished && !_hasTriggeredGameOver)
            {
                _hasTriggeredGameOver = true; // 🔒 Lock it so it runs once

                // A. Get My Identity
                var mySessionKey = SessionKeyManager.Instance.SessionAccount.PublicKey;

                // B. Check Who Won
                // Note: state.Winner is nullable, so we use ?.Equals
                bool iWon = state.Winner != null && state.Winner.Equals(mySessionKey);

                Debug.Log($"🏁 GAME OVER! Winner on Chain: {state.Winner}");
                Debug.Log(iWon ? "🎉 VICTORY! You won the pot!" : "💀 DEFEAT! Better luck next time.");

                // C. Trigger Visuals (UI)
                var gameController = FindFirstObjectByType<SumoGameController>();
                if (gameController != null)
                {
                    // You need to ensure SumoGameController has a public method for this
                    gameController.ShowGameResult(iWon);
                }

                // D. Auto-Claim (Optional / Advanced)
                if (iWon)
                {
                    Debug.Log("💰 Enabling 'Claim Prize' Button...");
                    // FindFirstObjectByType<Matchmaker>().ShowClaimButton();
                }
            }
        }

        // 3. Handle Input (Write)
        public void SendMove(sbyte x, sbyte y)
        {
            _activeProvider?.SendInput(x, y);
        }
    }
}