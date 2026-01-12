using UnityEngine;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;
using System.Threading.Tasks;

namespace PlaceholderHack.Networking
{
    public class SessionKeyManager : MonoBehaviour
    {
        public static SessionKeyManager Instance;

        // This is the "Burner" wallet for high-frequency moves
        public Account SessionAccount { get; private set; }

        // Check if we are ready to play
        public bool IsSessionValid => SessionAccount != null;

        void Awake()
        {
            if (Instance == null) Instance = this;
        }

        void Start()
        {
            // FOR TESTING ONLY: Auto-generate key on startup
            GenerateNewSession();
        }

        // Call this when entering the Lobby
        public void GenerateNewSession()
        {
            // 1. Create a fresh keypair in memory
            SessionAccount = new Account();
            Debug.Log($"🔑 Generated Session Key: {SessionAccount.PublicKey}");
        }

        // In a real MagicBlock implementation, this would send a
        // "Delegate" transaction to the on-chain program.
        // For this Hackathon MVP, we will sign directly with this key
        // assuming the Engine allows it (or we skip delegation for local testing).
        public Transaction SignTransaction(Transaction tx)
        {
            if (!IsSessionValid)
            {
                Debug.LogError("❌ No Session Key! Cannot sign move.");
                return null;
            }

            // Sign the transaction with the in-memory private key
            tx.Sign(SessionAccount);
            return tx;
        }
    }
}