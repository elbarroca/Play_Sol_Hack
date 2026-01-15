# 🚀 Hard-Stakes: PSG1 Hackathon Status Report

**Date:** January 15, 2026
**Status:** 🟢 **LIVE ON DEVNET** (Physics Loop Validated)
**Engine:** MagicBlock Ephemeral Rollups
**Cluster:** Devnet

---

## ✅ Accomplishments (What is done)

### 1. 🏎️ The Engine (Physics Module) - **VALIDATED**
*   **Deployment Success:** Engine Program `2JfW...` is live.
*   **Discriminator Handshake:** Solved the critical `InstructionFallbackNotFound` error by manually extracting the instruction discriminator (`11-3A-44-DD...`) from the on-chain IDL.
*   **Hardware-Native Inputs:** PSG1 Joystick inputs are successfully decoding on-chain via `MagicBlockClient`.
*   **Physics Loop:** 
    1. Unity sends Input bytes (`Sequence`, `x`, `y`).
    2. Blockchain processes logic.
    3. Account Data updates.
    4. Unity polls and snaps visual position.
    *   **Status:** ✅ The ball moves via Blockchain!

### 2. 🎮 The Client (Unity Bridge)
*   **Network Manager:** Configured for "Real Mode" (Mock Mode Unchecked).
*   **Session Management:** `SessionKeyManager` generates ephemeral keys in memory.
*   **Input Pipeline:** `PlayerInputBridge` captures Analog Stick, converts to `i8`, signs with Session Key, and transmits to RPC.
*   **Byte Debugging:** Validated exact hex payload transmission in Console.

### 3. 🏦 The Bank (Escrow Module)
*   **Zero-Trust Vaults:** Implemented PDA-based vaults in Rust.
*   **State Machine:** Enforces `Waiting` -> `Active` -> `Completed`.
*   **Settlement Logic:** Distributes 98% to winner / 2% to house.
*   **Status:** Deployed (`J5ju...`). *Pending Unity UI Integration.*

---

## 🛠️ Architecture Decisions (The "Why")

| Decision | Reason |
| :--- | :--- |
| **MagicBlock Rollups** | Standard Solana (400ms) is too slow. We validated ~50ms updates via Ephemeral validators. |
| **Session Keys** | We use a "Burner Wallet" pattern. The user connects their Main Wallet for identity, but a local Session Key signs the 20Hz movement packets to avoid popup fatigue. |
| **Manual Discriminator** | The IDL on Devnet used a custom discriminator. We hardcoded the 8-byte header in C# to force the handshake. |
| **Server Authority** | Unity is a "Dumb Terminal". The Rust program is the Database and the Physics Engine. |

---

## 🔮 Current Phase: The "Casino" Integration

**Status:** 🚧 Converting Tech Demo into Multiplayer DApp

### Phase 1: The "JIT" Funding (Piggyback)
**Goal:** Automate Session Key funding so users don't need CLI.
*   **Solution:** Create a single transaction containing:
    1.  `Bank.CreateMatch` (0.1 SOL to Vault).
    2.  `System.Transfer` (0.005 SOL to Session Key).
*   **Result:** Player bets and "fuels" their controller in one click.

### Phase 2: Multiplayer Handshake
**Goal:** P1 creates, P2 joins.
*   **P1:** Calls `InitGame` -> Status `Waiting`.
*   **P2:** Calls `JoinGame` -> Status `Active`.
*   **Scaling:** Every match has a unique `GameState` account address.

### Phase 3: The "Win" Signal
**Goal:** Detect outcome.
*   Rust Engine calculates `Distance > Radius` -> Sets `Status = Finished`.
*   Unity detects change -> Shows "Claim Prize" button.
*   Button calls `Bank.SettleMatch`.

---

## 🚨 Known Issues & Fixes

| Issue | Status | Fix |
| :--- | :--- | :--- |
| `InstructionFallbackNotFound` | ✅ Fixed | Hardcoded IDL Discriminator bytes in `MagicBlockClient.cs`. |
| NullReference in Network | ✅ Fixed | Assigned `MockClient` reference even in Real Mode to prevent `OnDisable` crash. |