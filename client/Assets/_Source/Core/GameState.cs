using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string publicKey;
    public ulong balance;
    public int score;
    public Vector3 position;
    public bool isConnected;
    public DateTime lastUpdate;
}

[Serializable]
public class GameSession
{
    public string sessionId;
    public string gameType;
    public List<PlayerData> players;
    public bool isActive;
    public DateTime startTime;
    public int maxPlayers;
}

public class GameState : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private string gameType = "PlaceholderGame";

    public static GameState Instance { get; private set; }

    private GameSession currentSession;
    private Dictionary<string, PlayerData> playerRegistry;

    public event Action<GameSession> OnSessionStarted;
    public event Action<GameSession> OnSessionEnded;
    public event Action<PlayerData> OnPlayerJoined;
    public event Action<PlayerData> OnPlayerLeft;
    public event Action<PlayerData> OnPlayerUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeState()
    {
        playerRegistry = new Dictionary<string, PlayerData>();
        Debug.Log("GameState initialized");
    }

    public void StartNewSession()
    {
        currentSession = new GameSession
        {
            sessionId = Guid.NewGuid().ToString(),
            gameType = gameType,
            players = new List<PlayerData>(),
            isActive = true,
            startTime = DateTime.Now,
            maxPlayers = maxPlayers
        };

        OnSessionStarted?.Invoke(currentSession);
        Debug.Log($"New game session started: {currentSession.sessionId}");
    }

    public void EndSession()
    {
        if (currentSession != null)
        {
            currentSession.isActive = false;
            OnSessionEnded?.Invoke(currentSession);
            currentSession = null;
            Debug.Log("Game session ended");
        }
    }

    public bool AddPlayer(string publicKey, PlayerData playerData)
    {
        if (currentSession == null || currentSession.players.Count >= maxPlayers)
        {
            Debug.LogWarning("Cannot add player: session full or not started");
            return false;
        }

        if (playerRegistry.ContainsKey(publicKey))
        {
            Debug.LogWarning($"Player {publicKey} already exists");
            return false;
        }

        playerData.publicKey = publicKey;
        playerData.lastUpdate = DateTime.Now;
        playerRegistry[publicKey] = playerData;
        currentSession.players.Add(playerData);

        OnPlayerJoined?.Invoke(playerData);
        Debug.Log($"Player {publicKey} joined the game");
        return true;
    }

    public void RemovePlayer(string publicKey)
    {
        if (playerRegistry.TryGetValue(publicKey, out PlayerData player))
        {
            playerRegistry.Remove(publicKey);
            currentSession?.players.Remove(player);
            player.isConnected = false;

            OnPlayerLeft?.Invoke(player);
            Debug.Log($"Player {publicKey} left the game");
        }
    }

    public void UpdatePlayer(string publicKey, PlayerData updatedData)
    {
        if (playerRegistry.TryGetValue(publicKey, out PlayerData player))
        {
            updatedData.publicKey = publicKey;
            updatedData.lastUpdate = DateTime.Now;
            playerRegistry[publicKey] = updatedData;

            // Update in session players list
            int index = currentSession.players.FindIndex(p => p.publicKey == publicKey);
            if (index >= 0)
            {
                currentSession.players[index] = updatedData;
            }

            OnPlayerUpdated?.Invoke(updatedData);
        }
    }

    public PlayerData GetPlayer(string publicKey)
    {
        return playerRegistry.TryGetValue(publicKey, out PlayerData player) ? player : null;
    }

    public List<PlayerData> GetAllPlayers()
    {
        return currentSession?.players ?? new List<PlayerData>();
    }

    public GameSession GetCurrentSession()
    {
        return currentSession;
    }

    public bool IsSessionActive()
    {
        return currentSession != null && currentSession.isActive;
    }
}