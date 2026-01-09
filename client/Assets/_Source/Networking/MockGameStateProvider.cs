using UnityEngine;

namespace PlaceholderHack.Networking
{
    public class MockGameStateProvider : MonoBehaviour, IGameStateProvider
    {
        // Local state simulation
        private long _p1X = -200; // -2.00m
        private long _p1Y = 0;
        private long _p2X = 200;  // +2.00m
        private long _p2Y = 0;

        // Configuration
        public bool AmIPlayerOne = true;
        public int Speed = 10; // Speed multiplier simulating Engine logic

        public void SendInput(int x, int y)
        {
            // Simulate the Rust logic locally
            if (AmIPlayerOne)
            {
                _p1X += x * Speed; // Simply adding input to position
                _p1Y += y * Speed;
            }
            else
            {
                _p2X += x * Speed;
                _p2Y += y * Speed;
            }

            // In a real mock, you might add a 50ms Delay here to simulate network lag!
        }

        public (long, long, long, long) GetCoordinates()
        {
            return (_p1X, _p1Y, _p2X, _p2Y);
        }

        public bool IsGameActive() => true;
    }
}