# 🕹️ HARD-STAKES (Monorepo)

The Flagship PSG1 High-Frequency PVP Arena.
Built for PlaySolana Gen 1 Hardware | Powered by MagicBlock Ephemeral Rollups.

![alt text](https://img.shields.io/badge/license-MIT-blue)
![alt text](https://img.shields.io/badge/Unity-2022.3%20LTS-black)
![alt text](https://img.shields.io/badge/Solana-Mainnet-green)
![alt text](https://img.shields.io/badge/MagicBlock-Ephemeral-purple)

## ⚡ The Manifesto

We reject virtual joysticks. We reject 200ms latency. Hard-Stakes is a physics-based Sumo arena optimized exclusively for the PSG1 Android Handheld.

- **Hardware-Native**: Inputs are read directly from Analog Sticks via the PlaySolana SDK. No touch overlays.
- **Zero-Trust Architecture**: Clients are dumb renderers. The MagicBlock Ephemeral Rollup is the authoritative physics engine.
- **Sub-50ms Latency**: Gameplay state is delegated to a high-frequency validator, settling back to L1 only for payouts.

## 🏗️ Architecture

This repository follows a strict Monorepo structure to separate concerns between Game Logic and Protocol Logic.

```mermaid
graph TD
    A[Unity Client (PSG1)] -->|Signed Input Packets| B(MagicBlock Ephemeral Rollup)
    B -->|State Updates 20hz| A
    C[Solana L1 Bank] -->|Deposit Wager| B
    B -->|Settle Winner| C
```

### Directory Map

| Folder | Tech Stack | Responsibility |
|--------|------------|----------------|
| `blockchain/` | Rust, Anchor | The Backend. Contains two programs:<br>1. `bank`: Standard Escrow (L1)<br>2. `engine`: Physics Logic (MagicBlock) |
| `client/` | Unity, C# | The Frontend. Handles Interpolation, Rendering, and PSG1 Hardware Inputs. |
| `libs/` | Git Submodules | External SDKs (PlaySolana) kept outside the Unity Assets folder to prevent pollution. |

## 🚀 Quick Start

### 1. Prerequisites

- **Rust & Cargo**: `curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh`
- **Solana CLI**: `sh -c "$(curl -sSfL https://release.solana.com/stable/install)"`
- **Anchor AVM**: `cargo install --git https://github.com/coral-xyz/anchor avm --locked --force`
- **Unity Hub**: Install Editor 2022.3.x (LTS).
- **Node.js & Yarn**.

### 2. Installation (The "One-Time" Setup)

```bash
# 1. Clone the Repo
git clone <repo-url>
cd PLAY_SOL_HACK

# 2. Install External SDKs (PlaySolana)
mkdir -p libs
cd libs
git clone https://github.com/playsolana/PlaySolana.Unity-SDK.git
cd ..

# 3. Install Blockchain Dependencies
cd blockchain
yarn install
```

### 3. Unity Setup

1. Open Unity Hub.
2. Click **Add Project from Disk**.
3. Select the `client/` folder.
4. **Important**: Inside Unity, go to **Window → Package Manager → + → Add package from disk**.
5. Select `libs/PlaySolana.Unity-SDK/package.json`.

## 🛠️ Developer Commands

We use root-level scripts to manage the workflow. Run these from the root folder:

| Command | Description |
|---------|-------------|
| `npm run chain:build` | Compiles both Rust programs (Bank & Engine). |
| `npm run chain:test` | Runs TypeScript integration tests against a local validator. |
| `npm run chain:deploy` | Deploys programs to your configured cluster (Localnet/Devnet). |
| `npm run gen:idl` | Exports IDLs to the `blockchain/idl` folder for Unity to read. |

## 🧩 The Protocol (Deep Dive)

### Program 1: The Bank (placeholder-bank)
- **Address**: Fg6Pa... (Devnet)
- **Function**: Holds user SOL in an escrow PDA.
- **Logic**: CreateMatch → JoinMatch → SettleMatch.

### Program 2: The Engine (placeholder-engine)
- **Address**: 8HoQn... (Devnet)
- **Function**: Calculates velocity, collision, and win conditions.
- **Magic**: Uses the `#[ephemeral]` macro to skip Solana consensus for gameplay ticks, achieving 50ms block times.

## 📦 Unity Assets Structure

We keep the Assets folder clean by strictly separating source code from plugins.

- `Assets/_Source/Core`: Game State definitions (mirrors Rust structs).
- `Assets/_Source/Networking`: Wallet Adapters & WebSocket logic.
- `Assets/_Source/Input`: PSG1 Hardware listeners.
- `Assets/_Source/Physics`: Visual interpolation (smoothing).

## 📝 License

MIT License. Built for the PlaySolana Gen 1 Hackathon.