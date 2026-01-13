using UnityEngine;
using PlaceholderHack.Networking;
using PlaceholderHack.Core;
using Solana.Unity.Wallet;

namespace PlaceholderHack.Games.Sumo
{
    public class SumoGameController : MonoBehaviour
    {
        [Header("System References")]
        public NetworkManager Network;

        [Header("UI References")]
        public GameObject WaitingScreen; // The "Looking for Match" label
        public GameObject VictoryScreen; // The "You Won" poster
        public GameObject DefeatScreen;  // The "Failed" poster

        private bool _gameEnded = false;

        void Start()
        {
            // 1. Initial UI State
            if (WaitingScreen) WaitingScreen.SetActive(true);
            if (VictoryScreen) VictoryScreen.SetActive(false);
            if (DefeatScreen) DefeatScreen.SetActive(false);
        }

        void Update()
        {
            // Stop logic if game is over or network isn't ready
            if (_gameEnded || Network == null) return;

            // 2. Fetch State (Abstracted for Mock vs Real)
            GameStateAccount state = GetCurrentState();

            // If we haven't received data yet, do nothing
            if (state == null) return;

            // 3. State Machine
            switch (state.Status)
            {
                case GameStatus.Waiting:
                    // Keep Waiting Screen Active
                    if (!WaitingScreen.activeSelf) WaitingScreen.SetActive(true);
                    break;

                case GameStatus.Active:
                    // Game Started -> Hide Waiting Screen
                    if (WaitingScreen.activeSelf) WaitingScreen.SetActive(false);
                    break;

                case GameStatus.Finished:
                    // Trigger End Sequence
                    HandleGameOver(state);
                    break;
            }
        }

        private GameStateAccount GetCurrentState()
        {
            // intelligently fetch state based on the flag in NetworkManager
            if (Network.UseMockMode)
            {
                return Network.MockClient != null ? Network.MockClient.CurrentState : null;
            }
            else
            {
                return Network.RealClient != null ? Network.RealClient.CurrentState : null;
            }
        }

        private void HandleGameOver(GameStateAccount state)
        {
            _gameEnded = true;
            bool didIWin = false;

            if (Network.UseMockMode)
            {
                // MOCK LOGIC:
                // FIX: Explicitly cast everything to long to make the compiler happy
                long p1X = state.P1Coords[0];
                long p1Z = state.P1Coords[1];
                long radius = (long)state.MapRadius; // <--- THE FIX: Cast ulong to long

                long p1DistSq = (p1X * p1X) + (p1Z * p1Z);
                long radiusSq = radius * radius;

                // If we are INSIDE the ring, we won (implies P2 fell)
                didIWin = p1DistSq <= radiusSq;
            }
            else
            {
                // REAL LOGIC:
                // Compare the "Winner" public key from chain against our Session Key
                var myKey = SessionKeyManager.Instance.SessionAccount.PublicKey;
                if (state.Winner != null && state.Winner.Equals(myKey))
                {
                    didIWin = true;
                }
            }

            Debug.Log($"🏁 GAME OVER. Winner: {(didIWin ? "ME" : "OPPONENT")}");

            // 4. Show Result UI
            if (VictoryScreen) VictoryScreen.SetActive(didIWin);
            if (DefeatScreen) DefeatScreen.SetActive(!didIWin);
            if (WaitingScreen) WaitingScreen.SetActive(false);
        }
    }
}