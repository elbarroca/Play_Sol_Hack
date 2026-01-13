using PlaceholderHack.Core;

namespace PlaceholderHack.Networking
{
    public interface IGameStateProvider
    {
        // The current snapshot of the game
        GameStateAccount CurrentState { get; }

        // Send a move command (x, y are -100 to 100)
        void SendInput(sbyte x, sbyte y);

        // Check if we are ready
        bool IsConnected { get; }
    }
}