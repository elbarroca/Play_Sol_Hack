using UnityEngine;
using PlaceholderHack.Core;
using Solana.Unity.Wallet; // For dummy keys

namespace PlaceholderHack.Networking
{
    public class MockGameStateProvider : MonoBehaviour, IGameStateProvider
    {
        public GameStateAccount CurrentState { get; private set; }
        public bool IsConnected => true;

        [Header("Simulation Settings")]
        public float MockLatency = 0.05f; // Simulate 50ms network lag

        private PublicKey _dummyP1 = new PublicKey("11111111111111111111111111111111");
        private PublicKey _dummyP2 = new PublicKey("22222222222222222222222222222222");

        void Awake()
        {
            // Initialize a fake game state (Matches Rust InitGame)
            CurrentState = new GameStateAccount
            {
                PlayerOne = _dummyP1,
                PlayerTwo = _dummyP2,
                P1Coords = new long[] { -200, 0 }, // Start Left
                P2Coords = new long[] { 200, 0 },  // Start Right
                MapRadius = 500,
                Status = GameStatus.Active,
                FrameCount = 0
            };
        }

        public void SendInput(sbyte x, sbyte y)
        {
            // Simulate the Rust Logic locally
            // Rust: coords[0] += (x * speed)
            // Speed = 10

            long speed = 10;

            // Update P1 (We pretend we are always P1 in Mock Mode)
            CurrentState.P1Coords[0] += (long)x * speed;
            CurrentState.P1Coords[1] += (long)y * speed;

            // Simple Ring Out Check (Pythagoras)
            long distSq = (CurrentState.P1Coords[0] * CurrentState.P1Coords[0]) +
                          (CurrentState.P1Coords[1] * CurrentState.P1Coords[1]);

            if (distSq > (long)(CurrentState.MapRadius * CurrentState.MapRadius))
            {
                CurrentState.Status = GameStatus.Finished;
                Debug.Log("🏆 Mock Game Over! Ring Out.");
            }

            CurrentState.FrameCount++;
        }

        // Optional: Simple AI for P2 to test collisions
        void Update()
        {
            // Make P2 move back and forth slightly
            float sin = Mathf.Sin(Time.time * 2f) * 50f;
            CurrentState.P2Coords[0] = 200 + (long)sin;
        }
    }
}