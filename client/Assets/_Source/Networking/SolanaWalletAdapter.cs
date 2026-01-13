using UnityEngine;
using System;
using System.Threading.Tasks;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Models;

namespace PlaceholderHack.Networking
{
    public class SolanaWalletAdapter : MonoBehaviour
    {
        [Header("Wallet Configuration")]
        [SerializeField] private string defaultDerivationPath = "m/44'/501'/0'/0'";

        public static SolanaWalletAdapter Instance { get; private set; }

        private Wallet wallet;
        private Account currentAccount;

        public event Action<Account> OnWalletConnected;
        public event Action OnWalletDisconnected;
        public event Action<ulong> OnBalanceChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // FIXED: Returns Task.FromResult to satisfy Task<bool> without async overhead
        public Task<bool> CreateWallet(string mnemonic = null)
        {
            try
            {
                if (string.IsNullOrEmpty(mnemonic))
                {
                    var generatedMnemonic = new Mnemonic(WordList.English, WordCount.Twelve);
                    mnemonic = generatedMnemonic.ToString();
                    Debug.Log($"Generated new wallet mnemonic: {mnemonic}");
                }

                wallet = new Wallet(mnemonic);
                currentAccount = wallet.GetAccount(0);

                PlayerPrefs.SetString("WalletMnemonic", mnemonic);
                PlayerPrefs.Save();

                OnWalletConnected?.Invoke(currentAccount);
                Debug.Log($"Wallet created for address: {currentAccount.PublicKey}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create wallet: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        // FIXED: Logic handles Task returns explicitly
        public Task<bool> LoadWallet()
        {
            try
            {
                string savedMnemonic = PlayerPrefs.GetString("WalletMnemonic", null);
                if (string.IsNullOrEmpty(savedMnemonic))
                {
                    Debug.LogWarning("No saved wallet found");
                    return Task.FromResult(false); // <--- THIS WAS YOUR ERROR
                }

                return CreateWallet(savedMnemonic);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load wallet: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public void DisconnectWallet()
        {
            wallet = null;
            currentAccount = null;
            PlayerPrefs.DeleteKey("WalletMnemonic");
            PlayerPrefs.Save();

            OnWalletDisconnected?.Invoke();
            Debug.Log("Wallet disconnected");
        }

        public Account GetCurrentAccount() => currentAccount;
        public string GetPublicKey() => currentAccount?.PublicKey?.ToString();
        public bool IsWalletConnected() => currentAccount != null;

        public async Task<ulong> GetBalance()
        {
            if (!IsWalletConnected()) return 0;

            try
            {
                var rpc = ClientFactory.GetClient(Cluster.DevNet);
                var balanceResult = await rpc.GetBalanceAsync(GetPublicKey());

                ulong balance = balanceResult.Result.Value;
                OnBalanceChanged?.Invoke(balance);
                return balance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get balance: {ex.Message}");
                return 0;
            }
        }

        public byte[] SignTransaction(byte[] transactionData)
        {
            if (!IsWalletConnected()) return null;
            try
            {
                return currentAccount.Sign(transactionData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to sign transaction: {ex.Message}");
                return null;
            }
        }
    }
}