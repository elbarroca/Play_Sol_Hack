using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Solana.Unity.Wallet; // Ensure you have Solana.Unity.Wallet namespace

namespace PlaceholderHack.Core
{
    public enum GameStatus
    {
        Waiting = 0,
        Active = 1,
        Finished = 2
    }

    public class GameStateAccount
    {
        // Offsets based on Rust Struct:
        // Discriminator: 8
        // player_one: 32
        // player_two: 33 (1 byte option + 32 bytes key)
        // p1_coords: 16 (8 bytes x 2)
        // p2_coords: 16 (8 bytes x 2)
        // map_radius: 8
        // game_status: 1
        // winner: 33 (1 byte option + 32 bytes key)
        // frame_count: 8

        public PublicKey PlayerOne;
        public PublicKey PlayerTwo;
        public long[] P1Coords;
        public long[] P2Coords;
        public ulong MapRadius;
        public GameStatus Status;
        public PublicKey Winner;
        public ulong FrameCount;

        public static GameStateAccount Deserialize(byte[] data)
        {
            if (data.Length < 100) return null; // Safety check

            ReadOnlySpan<byte> span = data.AsSpan();
            int offset = 8; // Skip 8-byte Anchor Discriminator

            GameStateAccount state = new GameStateAccount();

            // 1. Player One (32 bytes)
            state.PlayerOne = new PublicKey(span.Slice(offset, 32).ToArray());
            offset += 32;

            // 2. Player Two (Option<Pubkey> = 1 + 32)
            bool hasP2 = span[offset] == 1;
            offset += 1;
            if (hasP2)
            {
                state.PlayerTwo = new PublicKey(span.Slice(offset, 32).ToArray());
            }
            offset += 32; // Always skip 32 for alignment in Fixed Size accounts

            // 3. P1 Coords ([i64; 2] = 16 bytes)
            state.P1Coords = new long[2];
            state.P1Coords[0] = span.GetS64(offset); offset += 8;
            state.P1Coords[1] = span.GetS64(offset); offset += 8;

            // 4. P2 Coords ([i64; 2] = 16 bytes)
            state.P2Coords = new long[2];
            state.P2Coords[0] = span.GetS64(offset); offset += 8;
            state.P2Coords[1] = span.GetS64(offset); offset += 8;

            // 5. Map Radius (u64 = 8 bytes)
            state.MapRadius = span.GetU64(offset); offset += 8;

            // 6. Game Status (Enum = 1 byte)
            state.Status = (GameStatus)span[offset]; offset += 1;

            // 7. Winner (Option<Pubkey> = 1 + 32)
            bool hasWinner = span[offset] == 1;
            offset += 1;
            if (hasWinner)
            {
                state.Winner = new PublicKey(span.Slice(offset, 32).ToArray());
            }
            offset += 32;

            // 8. Frame Count (u64 = 8 bytes)
            state.FrameCount = span.GetU64(offset);

            return state;
        }
    }

    // Helper extension for reading bytes
    public static class SpanExtensions
    {
        public static long GetS64(this ReadOnlySpan<byte> span, int offset)
        {
            return BitConverter.ToInt64(span.Slice(offset, 8).ToArray(), 0);
        }
        public static ulong GetU64(this ReadOnlySpan<byte> span, int offset)
        {
            return BitConverter.ToUInt64(span.Slice(offset, 8).ToArray(), 0);
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