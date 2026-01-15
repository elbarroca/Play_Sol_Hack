using UnityEngine;
using UnityEngine.UI;
using Solana.Unity.SDK; // Access to Web3
using Solana.Unity.Wallet;
using TMPro;

public class LobbyController : MonoBehaviour
{
    [Header("UI References")]
    public Button StakeButton;       // The button that starts the game
    public GameObject LobbyPanel;    // The UI Overlay
    public GameObject GameView;      // The actual game camera/objects
    public TextMeshProUGUI BalanceText; 

    void Start()
    {
        // 1. Lock the game initially
        StakeButton.interactable = false; 
        GameView.SetActive(false);
        LobbyPanel.SetActive(true);

        // 2. Listen for Wallet Connection
        Web3.OnLogin += OnWalletLoggedIn;
        Web3.OnLogout += OnWalletLoggedOut;
    }

    private void OnWalletLoggedIn(Account account)
    {
        Debug.Log("✅ Main Wallet Connected: " + account.PublicKey);
        
        // Enable the Stake button because we are now authenticated
        StakeButton.interactable = true;
        
        // Optional: Show SOL Balance
        UpdateBalance();
    }

    private void OnWalletLoggedOut()
    {
        StakeButton.interactable = false;
        if(BalanceText) BalanceText.text = "Connect Wallet";
    }

    public void OnStakePressed()
    {
        // 3. START GAME SEQUENCE
        Debug.Log("💰 Staking 0.1 SOL... (Placeholder for Part 3)");
        
        // Hide UI, Show Game
        LobbyPanel.SetActive(false);
        GameView.SetActive(true);

        // Generate the Session Key for Movement
        var sessionMgr = FindObjectOfType<PlaceholderHack.Networking.SessionKeyManager>();
        if(sessionMgr != null) sessionMgr.GenerateNewSession();
    }

    private async void UpdateBalance()
    {
        if(BalanceText)
        {
            var balance = await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey);
            double sol = (double)balance.Result.Value / 1000000000;
            BalanceText.text = $"{sol:F2} SOL";
        }
    }
}