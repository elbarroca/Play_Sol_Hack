using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlaceholderHack.Core
{
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance;

        // State passed from Lobby to Game
        public string CurrentMatchID;
        public GameType SelectedGame;
        public bool AmIPlayerOne;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keeps this alive when scene changes
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartGame(GameType game, string matchId, bool isP1)
        {
            SelectedGame = game;
            CurrentMatchID = matchId;
            AmIPlayerOne = isP1;

            if (game == GameType.Tanks) SceneManager.LoadScene("Tanks");
            if (game == GameType.Sumo) SceneManager.LoadScene("Sumo");
        }
    }

    public enum GameType { Tanks, Sumo }
}