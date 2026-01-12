# 🚀 Hard-Stakes: PSG1 Hackathon Status Report

**Date:** January 12, 2026
**Status:** Session Keys Active / Input Bridge Ready / Physics Engine Validated
**Engine:** MagicBlock Ephemeral Rollups (Sub-50ms Latency)

---

## ✅ Accomplishments (What is done)

### 1. 🏦 The Bank (Escrow Module)
*   **Zero-Trust Vaults:** Implemented PDA-based vaults. Funds are locked programmatically, not held by an admin wallet.
*   **State Machine:** Enforces `Waiting` -> `Active` -> `Completed` lifecycle.
*   **Settlement Logic:** Automatically distributes 98% to winner / 2% to house.
*   **Verification:** Validated via Anchor TypeScript tests (`tests/bank.ts`).

### 2. 🏎️ The Engine (Physics Module)
*   **Hardware-Native Inputs:** Optimized for Play Solana Gen 1 (PSG1). Accepts raw `i8` inputs (-100 to 100) from analog sticks.
*   **Fixed-Point Math:** Solved the Floating Point Determinism issue by scaling coordinates (Unity `2.55f` = Blockchain `255`).
*   **Ring-Out Logic:** On-chain geometry checks (Pythagoras) to determine the winner authoritatively.
*   **Verification:** Validated via `tests/engine.ts`.

### 3. 🎮 The Client (Unity Bridge)
*   **Project Hygiene:** Performed a massive "Git Exorcism" (reduced 10,000+ cache files to <50 tracked source files).
*   **Serialization Bridge:** Created `GameState.cs` to deserialize raw Rust byte arrays into C# classes. Confirmed parity via Unit Tests.
*   **Session Management:** implemented `SessionKeyManager.cs`. The client now auto-generates ephemeral "Burner Wallets" on startup.
*   **Input Pipeline:** Built `PlayerInputBridge` to capture PSG1 Hardware Inputs, rate-limit them (20Hz), and sign transactions without user prompts.

---

## 🛠️ Architecture Decisions (The "Why")

| Decision | Reason |
| :--- | :--- |
| **MagicBlock Rollups** | Standard Solana (400ms) is too slow for physics. Rollups provide <50ms ticks. |
| **Session Keys** | Asking the user to approve every movement transaction is bad UX. Ephemeral keys allow "spamming" inputs securely. |
| **Server Authority** | The Client sends *inputs*, the Blockchain calculates *position*. This prevents teleport/speed hacks. |
| **New Input System** | The PSG1 hardware is an Android Gamepad. We strictly use `UnityEngine.InputSystem` to map the analog sticks correctly. |

---

## 🔮 Immediate Next Steps

The code is written, compiled, and wired. The final step is connecting the two worlds.

1.  **Deploy to Devnet:**
    *   Upload the Rust Engine to Solana Devnet (`anchor deploy`).
    *   This provides the `Program ID` and allows us to create a `Game State Account`.

2.  **Link the Address:**
    *   Copy the `Game State Public Key` from the deploy script.
    *   Paste it into the `MagicBlockClient` component in the Unity Inspector.

3.  **The First Move:**
    *   Press Play.
    *   Move the Joystick.
    *   Observe the coordinates updating on the Blockchain Explorer (or console logs).

---

## 👨‍💻 Developer Notes

*   **Repository Cleanliness:** The `.gitignore` is set to strict mode. Do NOT force commit `Library/` or `Temp/` folders.
*   **Testing:** Use `HardStakes -> Test Data Contract` in the Unity Editor menu to verify struct alignment if you change the Rust code.

---

## 🗺️ Hard-Stakes: The "Endgame" Roadmap

**Objective:** Close the loop (Input → Chain → Visuals → Payout).

### PHASE 1: The "Visual Sync" (Chain → Unity)

Currently, we send inputs (Write), but we are blindly trusting the void. We need to **Read** the result to move the ball.

#### 1.1 Implement State Polling
We need the client to ask the blockchain: "Where am I?" every frame (or every 100ms).

**Task:** Update `MagicBlockClient.cs` to implement a `PollGameState` loop.

**Logic:**
- Call `rpc.GetAccountInfo(GameStateAddress)`
- Deserialize the data using `GameStateAccount.Deserialize`
- Extract `P1Coords`/`P2Coords`
- Send coordinates to `NetworkSumo.cs`

#### 1.2 The Interpolation Fix
**Task:** Connect the `NetworkSumo` script to the `MagicBlockClient`.

**Why:** When polling says "You are at X=100", `NetworkSumo` must smoothly glide the ball from current spot to 1.0f.

**Action:** Ensure `NetworkSumo.OnServerUpdate` is called when data arrives.

### PHASE 2: The "Lobby" Flow (Bank → Engine)

We need to link the money to the match.

#### 2.1 The "Wager" Transaction
**Task:** Create a UI Button "Create Match (0.1 SOL)".

**Action:**
- Unity generates a MatchID (Random u64)
- Unity calls `Bank.CreateMatch`
- Unity calls `Engine.InitGame` using the same MatchID (or deriving the address deterministically)
- **Crucial:** Pass the MatchID to Engine so it knows which Bank Vault it belongs to

#### 2.2 The "Handshake"
**Task:** Verify Player 2 can join.

**Action:**
- P2 enters the MatchID
- Unity calls `Bank.JoinMatch`
- Unity calls `Engine.JoinGame` (add this instruction to Rust if P2 was hardcoded in tests)

### PHASE 3: The "Settlement" Bridge (Engine → Bank)

This is the "Money Shot". How does the Bank know the Engine finished?

#### 3.1 The "Game Over" Trigger
**Scenario:** P1 pushes P2 out of the ring.

**Chain Event:** Engine updates state: `game_status = Finished`, `winner = P1_Pubkey`.

#### 3.2 The Settlement Crank
Since Solana programs cannot auto-execute, the Client must trigger the payout.

**Task:** Create a `SettlementManager.cs`.

**Logic:**
- Watch `GameState.Status`
- If `Status == Finished`:
  - Read `GameState.Winner`
  - Construct transaction for `Bank.SettleMatch`
- **Important:** Transaction takes GameState account as input so Bank can verify winner trustlessly

*(Note: For Hackathon MVP, passing winner as argument is acceptable if time is tight, but reading the account is the "Mainnet Standard").*

---

## 🛠️ Technical Execution Plan (Next 48 Hours)

### Step 1: The Polling Loop (Code to Copy)

Add this to your `MagicBlockClient.cs` to make the ball move visually:

```csharp
// Inside MagicBlockClient class
private bool _isPolling = false;

public async void StartPolling()
{
    _isPolling = true;
    while (_isPolling && _gameStateKey != null)
    {
        try
        {
            // 1. Fetch Account Info
            var accountInfo = await _rpc.GetAccountInfoAsync(_gameStateKey);
            if (accountInfo.Result.Value != null)
            {
                // 2. Decode Data (Base64 -> Byte[] -> C# Class)
                byte[] data = Convert.FromBase64String(accountInfo.Result.Value.Data[0]);
                var state = PlaceholderHack.Core.GameStateAccount.Deserialize(data);

                // 3. Broadcast to Visuals
                // (Assuming simple singleton access for prototype)
                var sumo = FindFirstObjectByType<PlaceholderHack.Games.Sumo.NetworkSumo>();
                if (sumo != null)
                {
                    // Pass P1 or P2 depending on identity
                    // For now, let's visualize P1
                    sumo.OnServerUpdate(state.P1Coords[0], state.P1Coords[1]);
                }
            }
        }
        catch (Exception e) { Debug.LogWarning($"Polling Error: {e.Message}"); }

        // 4. Wait 100ms (10Hz refresh) - Adjust for MagicBlock speed
        await UniTask.Delay(100);
    }
}
```

### Step 2: Session Funding

**Issue:** Your Session Key (`SessionKeyManager`) is a new wallet. It has 0 SOL.

**Problem:** It cannot pay gas fees to move.

**Solution (Hackathon):**
When `GenerateNewSession()` runs...
Immediately invoke `SolanaWalletAdapter.Instance.Transfer(0.01 SOL)` from the Main Wallet to the Session Wallet.

This "fuels" the burner wallet for the match.

### Step 3: Deployment

- Deploy Engine to Devnet
- Deploy Bank to Devnet
- Update Configs: Paste the new Program IDs into `Anchor.toml`, `lib.rs`, and Unity Inspector

---

## 🔮 Summary

You have the skeleton.

**Fuel the Session Key** (Transfer SOL).  
**Read the State** (Polling Loop).  
**Connect the Bank** (Settlement Trigger).

**Command:** Implement the Polling Loop in `MagicBlockClient.cs` next. That gives you the visual feedback required to debug the rest.