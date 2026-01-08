using UnityEngine;
using System;
using System.Threading.Tasks;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;

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

    public async Task<bool> CreateWallet(string mnemonic = null)
    {
        try
        {
            if (string.IsNullOrEmpty(mnemonic))
            {
                // Generate new mnemonic
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

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create wallet: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> LoadWallet()
    {
        try
        {
            string savedMnemonic = PlayerPrefs.GetString("WalletMnemonic", null);
            if (string.IsNullOrEmpty(savedMnemonic))
            {
                Debug.LogWarning("No saved wallet found");
                return false;
            }

            return await CreateWallet(savedMnemonic);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load wallet: {ex.Message}");
            return false;
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

    public Account GetCurrentAccount()
    {
        return currentAccount;
    }

    public string GetPublicKey()
    {
        return currentAccount?.PublicKey?.ToString();
    }

    public bool IsWalletConnected()
    {
        return currentAccount != null;
    }

    public async Task<ulong> GetBalance()
    {
        if (!IsWalletConnected())
        {
            Debug.LogWarning("Wallet not connected");
            return 0;
        }

        try
        {
            ulong balance = await MagicBlockClient.Instance.GetBalanceAsync(GetPublicKey());
            OnBalanceChanged?.Invoke(balance);
            return balance;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get balance: {ex.Message}");
            return 0;
        }
    }

    public string SignMessage(string message)
    {
        if (!IsWalletConnected())
        {
            Debug.LogWarning("Wallet not connected");
            return null;
        }

        try
        {
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
            byte[] signature = currentAccount.Sign(messageBytes);
            return Convert.ToBase64String(signature);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to sign message: {ex.Message}");
            return null;
        }
    }

    public byte[] SignTransaction(byte[] transactionData)
    {
        if (!IsWalletConnected())
        {
            Debug.LogWarning("Wallet not connected");
            return null;
        }

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

    // Utility method to get wallet address for display
    public string GetShortAddress(int length = 8)
    {
        if (!IsWalletConnected()) return "Not Connected";

        string fullAddress = GetPublicKey();
        if (fullAddress.Length <= length) return fullAddress;

        return $"{fullAddress.Substring(0, length / 2)}...{fullAddress.Substring(fullAddress.Length - length / 2)}";
    }
}