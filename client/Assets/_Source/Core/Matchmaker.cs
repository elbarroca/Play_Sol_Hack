using UnityEngine;
using Solana.Unity.SDK; // ✅ REQUIRED for Web3
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types; // ✅ REQUIRED for Commitment
using Solana.Unity.Programs;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlaceholderHack.Networking;
using System;

public class Matchmaker : MonoBehaviour
{
    // 🏦 BANK: The Escrow Program (J5ju...)
    public string BankProgramId = "J5juVUkDeUwb3rXT1saab2q87aXyBjzmuAZ7dhMCfbka"; 

    // 🏎️ ENGINE: The Physics Program (2JfW...)
    public string ProgramId = "2JfW8D59eJ1myVbqpU8BBLxkWp3Bhwf6yjY8HfuqBSHv";
    
    // 🏟️ GAME STATE: The specific match data account
    public string GameStateAddress = "CYoh5qfXbvy2TcbBRsgkN9yJDmAoA13W5M75qHuEqEyi";

    // Call this when user clicks "STAKE & PLAY"
    public async void CreateMatchAndFundSession()
    {
        // 🛠️ FIX 1: Use Web3.Wallet (Static), NOT Web3.Instance.Wallet
        var mainWallet = Web3.Wallet;
        var rpc = Web3.Rpc;
        var mainAccount = Web3.Account; // ✅ Get the account public key from here

        if (mainWallet == null || mainAccount == null)
        {
            Debug.LogError("❌ Wallet not connected!");
            return;
        }

        // 1. Generate the Session Key (The Joystick)
        var sessionMgr = SessionKeyManager.Instance;
        sessionMgr.GenerateNewSession();
        var sessionKeyPub = sessionMgr.SessionAccount.PublicKey;

        Debug.Log($"🔑 Generated Session Key: {sessionKeyPub}");

        // 2. Decide: Init (Create) or Join?
        var gameStatePubKey = new PublicKey(GameStateAddress);
        var accountInfo = await rpc.GetAccountInfoAsync(gameStatePubKey);

        TransactionInstruction gameIx;
        
        if (accountInfo.Result.Value == null)
        {
            Debug.Log("🆕 Account missing. CREATING Match (InitGame)...");
            gameIx = BuildInitGameInstruction(sessionKeyPub, mainAccount.PublicKey);
        }
        else
        {
            Debug.Log("⚔️ Account exists. JOINING Match (JoinGame)...");
            gameIx = BuildJoinGameInstruction(sessionKeyPub, mainAccount.PublicKey);
        }

        // 3. Instruction A: Gas Funding (Main Wallet -> Session Key)
        var fundIx = SystemProgram.Transfer(
            mainAccount.PublicKey,
            sessionKeyPub,
            5000000 
        );

        // 4. Bundle Transaction (Fund + Game Logic)
        var tx = new Transaction()
        {
            FeePayer = mainAccount.PublicKey,
            Instructions = new List<TransactionInstruction> { fundIx, gameIx },
            RecentBlockHash = (await rpc.GetLatestBlockHashAsync()).Result.Value.Blockhash
        };

        // 5. Sign & Send (Standard Solana Unity SDK Pattern)
        var signedTx = await mainWallet.SignTransaction(tx);
        var res = await rpc.SendTransactionAsync(Convert.ToBase64String(signedTx.Serialize()), true, Commitment.Confirmed);

        if(res.WasSuccessful)
        {
            Debug.Log("✅ SUCCESS! Session Funded & Game Joined.");
        }
        else
        {
            Debug.LogError($"❌ Transaction Failed: {res.Reason}");
        }
    }

    private TransactionInstruction BuildInitGameInstruction(PublicKey sessionKey, PublicKey payer)
    {
        List<byte> data = new List<byte>();

        // 🔐 INIT Discriminator: "global:init_game"
        data.AddRange(new byte[] { 251, 46, 12, 208, 184, 148, 157, 73 });

        // Arg: Session Key
        data.AddRange(sessionKey.KeyBytes);

        var gameStatePubKey = new PublicKey(GameStateAddress);
        
        var accounts = new List<AccountMeta>
        {
            AccountMeta.Writable(gameStatePubKey, true),        
            AccountMeta.Writable(payer, true), 
            AccountMeta.ReadOnly(new PublicKey("11111111111111111111111111111112"), false) 
        };

        return new TransactionInstruction { ProgramId = new PublicKey(ProgramId), Keys = accounts, Data = data.ToArray() };
    }

    private TransactionInstruction BuildJoinGameInstruction(PublicKey sessionKey, PublicKey payer)
    {
        List<byte> data = new List<byte>();

        // 🔐 JOIN Discriminator: "global:join_game"
        data.AddRange(new byte[] { 107, 112, 18, 38, 56, 173, 60, 128 });

        // Arg: Session Key
        data.AddRange(sessionKey.KeyBytes);

        var gameStatePubKey = new PublicKey(GameStateAddress);

        var accounts = new List<AccountMeta>
        {
            AccountMeta.Writable(gameStatePubKey, false),       
            AccountMeta.Writable(payer, true)  
        };

        return new TransactionInstruction { ProgramId = new PublicKey(ProgramId), Keys = accounts, Data = data.ToArray() };
    }
}