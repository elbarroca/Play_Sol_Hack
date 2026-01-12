# 📝 Hard-Stakes: Development To-Do

## 🟢 Completed (Phase 1: Architecture)
- [x] **Bank Module:** Escrow logic, Vault PDAs, and Fee splitting (Rust).
- [x] **Bank Tests:** Validated via Anchor/TypeScript.
- [x] **Project Cleanup:** Configured `.gitignore` and removed Unity Library bloat.
- [x] **Engine Module:** Implemented Fixed-Point Physics and "Ring Out" logic.
- [x] **Unity Networking:** Created `MagicBlockClient` to construct raw transactions.
- [x] **Unity Input:** Wired PSG1 Joystick to `PlayerInputBridge` with Rate Limiting.
- [x] **Session Keys:** Auto-generation of ephemeral signing keys on game start.

## 🟡 In Progress (Phase 2: The Loop)
- [ ] **State Polling:** Implement loop in Unity to fetch `AccountInfo` and deserialize coordinates.
- [ ] **Visual Interpolation:** Connect `MagicBlockClient` data to `NetworkSumo.cs` to move the ball visually.
- [ ] **Devnet Deployment:** Deploy final Engine code and generate a persistent Game State Address.
- [ ] **Lobby UI:** Create a simple UI to input Match ID and initialize the session.

## 🔴 Upcoming (Phase 3: Settlement)
- [ ] **Game Over Trigger:** Client detects "Finished" state from blockchain.
- [ ] **Settlement Transaction:** Construct the `settle_match` call in Unity.
- [ ] **Mainnet Prep:** Optimize Compute Units (CU) for the Engine.
- [ ] **Audio/Haptics:** Hook up `Gamepad.SetMotorSpeeds` for collision feedback.