using UnityEngine;
using Cysharp.Threading.Tasks; // Ensure UniTask is installed, or use Task

namespace PlaceholderHack.Networking
{
    public interface IGameStateProvider
    {
        // 1. Send Input (Fire and Forget)
        void SendInput(int x, int y);

        // 2. Get Current State (Polling)
        // Returns the positions as Fixed Point integers (x100)
        (long p1X, long p1Y, long p2X, long p2Y) GetCoordinates();

        // 3. Game Lifecycle
        bool IsGameActive();
    }
}