using UnityEngine;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;

public class SessionKeyManager : MonoBehaviour
{
    // The temporary keypair for the match
    public Account SessionAccount { get; private set; }

    public void GenerateNewSession()
    {
        // 1. Generate a fresh keypair in memory
        SessionAccount = new Account();
        Debug.Log($"🔑 Generated Session Key: {SessionAccount.PublicKey}");

        // 2. Store it securely (In memory only for hackathon is fine)
        // Ideally, we don't save this to disk to ensure it's ephemeral.
    }

    public TransactionInstruction CreateDelegateInstruction(PublicKey mainWallet, PublicKey gameProgramId)
    {
        // This prepares the TX for the main wallet to sign later.
        // Even without the Rust program, we can structure this logic.
        Debug.Log("📝 Creating Delegation Instruction...");
        return null; // Placeholder until IDL is loaded
    }

    public byte[] SignGamePacket(byte[] instructionData)
    {
        // This is what happens 20 times a second during gameplay
        if(SessionAccount == null) return null;
        return SessionAccount.Sign(instructionData);
    }
}