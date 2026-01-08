using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Models;

[System.Serializable]
public class NetworkGameState
{
    public long[] Player1Pos = new long[2];
    public long[] Player2Pos = new long[2];
    public long Player1Rotation;
    public long Player2Rotation;
}

public class MagicBlockClient : MonoBehaviour
{
    [Header("MagicBlock Configuration")]
    [SerializeField] private string rpcUrl = "https://api.magicblock.app/devnet";
    [SerializeField] private string websocketUrl = "wss://api.magicblock.app/devnet";

    private IRpcClient rpcClient;
    private IStreamingRpcClient streamingClient;

    public static MagicBlockClient Instance { get; private set; }
    public NetworkGameState CurrentState { get; private set; } = new NetworkGameState();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeClients();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeClients()
    {
        // Initialize RPC client for blockchain interactions
        rpcClient = ClientFactory.GetClient(rpcUrl);

        // Initialize streaming client for real-time updates
        streamingClient = ClientFactory.GetStreamingClient(websocketUrl);

        Debug.Log("MagicBlock clients initialized");
    }

    public async Task<AccountInfo> GetAccountInfoAsync(string accountAddress)
    {
        try
        {
            var accountInfo = await rpcClient.GetAccountInfoAsync(accountAddress);
            return accountInfo.Result?.Value;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get account info: {ex.Message}");
            return null;
        }
    }

    public async Task<ulong> GetBalanceAsync(string publicKey)
    {
        try
        {
            var balance = await rpcClient.GetBalanceAsync(publicKey);
            return balance.Result?.Value ?? 0;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get balance: {ex.Message}");
            return 0;
        }
    }

    public async Task<string> SendTransactionAsync(byte[] transaction)
    {
        try
        {
            var txResult = await rpcClient.SendTransactionAsync(transaction);
            return txResult.Result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send transaction: {ex.Message}");
            return null;
        }
    }

    private void OnDestroy()
    {
        // Cleanup connections
        streamingClient?.Dispose();
    }
}