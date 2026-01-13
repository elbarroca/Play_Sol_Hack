# 📝 Hard-Stakes: Development To-Do

## 🟢 Completed (Phase 1: Architecture)
- [x] **Bank Module:** Escrow logic, Vault PDAs, and Fee splitting (Rust).
- [x] **Bank Tests:** Validated via Anchor/TypeScript.
- [x] **Project Cleanup:** Configured `.gitignore` and removed Unity Library bloat.
- [x] **Engine Module:** Implemented Fixed-Point Physics and "Ring Out" logic.
- [x] **Provider Pattern:** Interface-based architecture (`IGameStateProvider`) for Mock/Real switching.
- [x] **Mock Provider:** Full Rust engine simulation with ring-out detection and AI opponent.
- [x] **Network Manager:** Traffic cop between providers with zero-deployment workflow.
- [x] **Unity Networking:** Created `MagicBlockClient` to construct raw transactions.
- [x] **Unity Input:** Wired PSG1 Joystick to `PlayerInputBridge` with Rate Limiting.
- [x] **Session Keys:** Auto-generation of ephemeral signing keys on game start.
- [x] **Game Controller:** Complete state machine (Waiting → Active → Finished) with UI management.
- [x] **Mock Loop Validation:** Full game loop tested and working offline.

## 🟡 In Progress (Phase 2: Deployment & Integration)
- [ ] **Fix Build Environment:** Resolve `cargo-build-sbf` resource error (Priority #1 Blocker).
- [ ] **Asset Swap:** Replace white sphere with actual Sumo character prefab.
- [ ] **Devnet Deployment:** Deploy final Engine code and generate Game State Address.
- [ ] **Lobby UI:** Create "Stake 0.1 SOL" button and match creation flow.
- [ ] **Bank-Engine Link:** Connect wager transactions to physics engine.

## 🔴 Upcoming (Phase 3: Settlement & Polish)
- [ ] **Settlement Logic:** Detect game over state and trigger fund distribution.
- [ ] **Winner Verification:** Read blockchain winner and construct settlement transaction.
- [ ] **Multiplayer Join:** Implement P2 match joining flow.
- [ ] **Audio/Haptics:** PSG1 rumble feedback for collisions and wins.
- [ ] **Mainnet Prep:** CU optimization and final deployment preparation.