using UnityEngine;
using Solana.Unity.Rpc;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;

namespace PlaceholderHack.Networking
{
    public class MagicBlockClient : MonoBehaviour
    {
        [Header("Configuration")]
        public string RpcUrl = "https://api.devnet.solana.com"; 
        public string ProgramId = "8HoQnePLqPj4M7PUDzwxKBUBLqjnZ3zDKcn78VFyF5zP"; 
        public string GameStateAddress; 

        private IRpcClient _rpc;
        private PublicKey _programId;
        private PublicKey _gameStateKey;
        
        void Awake()
        {
            // FIX 1: Use the custom URL string, or use Cluster.DevNet (Capital N)
            _rpc = ClientFactory.GetClient(RpcUrl);

            _programId = new PublicKey(ProgramId);

            if(!string.IsNullOrEmpty(GameStateAddress))
            {
                _gameStateKey = new PublicKey(GameStateAddress);
                StartPolling(); // <--- START THE LOOP
            }
        }

        public void SendMoveCommand(sbyte x, sbyte y)
        {
            if (_gameStateKey == null) return;
            
            var session = SessionKeyManager.Instance.SessionAccount;
            if (session == null) return;

            byte[] instructionData = BuildMovePlayerInstruction(x, y);

            var accounts = new List<AccountMeta>
            {
                AccountMeta.Writable(_gameStateKey, false),
                AccountMeta.ReadOnly(session.PublicKey, true)
            };

            TransactionInstruction ix = new TransactionInstruction
            {
                ProgramId = _programId,
                Keys = accounts,
                Data = instructionData
            };

            SendAndForget(ix, session).Forget();
        }

        private async UniTaskVoid SendAndForget(TransactionInstruction ix, Account signer)
        {
            try 
            {
                var blockHash = await _rpc.GetLatestBlockHashAsync();
                var tx = new Transaction
                {
                    RecentBlockHash = blockHash.Result.Value.Blockhash,
                    FeePayer = signer.PublicKey,
                    Instructions = new List<TransactionInstruction> { ix },
                    Signatures = new List<SignaturePubKeyPair>()
                };

                tx.Sign(signer);
                await _rpc.SendTransactionAsync(tx.Serialize(), true, Commitment.Processed);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Tx Failed: {e.Message}");
            }
        }

        private byte[] BuildMovePlayerInstruction(sbyte x, sbyte y)
        {
            List<byte> data = new List<byte>();
            // "global:move_player" discriminator
            data.AddRange(new byte[] { 46, 219, 237, 204, 23, 222, 235, 153 });
            data.Add((byte)x);
            data.Add((byte)y);
            return data.ToArray();
        }

        public async void StartPolling()
        {
            Debug.Log("👁️ Starting State Polling...");
            while (this != null && _gameStateKey != null) // Safety check
            {
                try
                {
                    // 1. Fetch Account Data from Solana
                    var accountInfo = await _rpc.GetAccountInfoAsync(_gameStateKey);

                    if (accountInfo.Result.Value != null)
                    {
                        // 2. Deserialize (Base64 -> C# Class)
                        byte[] data = Convert.FromBase64String(accountInfo.Result.Value.Data[0]);
                        var state = PlaceholderHack.Core.GameStateAccount.Deserialize(data);

                        // 3. Update The Visuals (NetworkSumo)
                        // Find the ball in the scene
                        var sumoBall = FindFirstObjectByType<PlaceholderHack.Games.Sumo.NetworkSumo>();

                        if (sumoBall != null)
                        {
                            // Send the X/Y coordinates to the interpolator
                            sumoBall.OnServerUpdate(state.P1Coords[0], state.P1Coords[1]);
                        }
                    }
                }
                catch (Exception e)
                {
                    // Suppress errors if we just haven't deployed yet
                }

                // 4. Wait 100ms (10Hz Refresh Rate)
                await UniTask.Delay(100);
            }
        }
    }
}