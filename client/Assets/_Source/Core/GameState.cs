using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Solana.Unity.Programs.Utilities; // Requires Solana.Unity SDK

namespace PlaceholderHack.Core
{
    // 1. Define the Enum to match Rust
    public enum GameStatus
    {
        Waiting = 0,
        Active = 1,
        Finished = 2
    }

    // 2. Define the Account Class (The Data Contract)
    public class GameStateAccount
    {
        // Rust: [i64; 2] -> C#: long[]
        public long[] P1Coords;
        public long[] P2Coords;
        public ulong MapRadius;
        public GameStatus Status;
        public ulong FrameCount;

        // 3. The Deserializer (Parsing the raw bytes from Solana)
        public static GameStateAccount Deserialize(byte[] data)
        {
            // --- FIX START: Convert array to Span ---
            ReadOnlySpan<byte> span = data.AsSpan();
            // ----------------------------------------

            int offset = 8; // Skip Anchor Discriminator

            GameStateAccount state = new GameStateAccount();

            // Read P1 Coords
            state.P1Coords = new long[2];
            state.P1Coords[0] = span.GetS64(offset); offset += 8; // <--- Use 'span' here
            state.P1Coords[1] = span.GetS64(offset); offset += 8;

            // Read P2 Coords
            state.P2Coords = new long[2];
            state.P2Coords[0] = span.GetS64(offset); offset += 8;
            state.P2Coords[1] = span.GetS64(offset); offset += 8;

            // Read Radius
            state.MapRadius = span.GetU64(offset); offset += 8;

            // Read Enum
            state.Status = (GameStatus)span.GetU8(offset); offset += 1;

            // Read Frame Count
            state.FrameCount = span.GetU64(offset); offset += 8;

            return state;
        }
    }

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