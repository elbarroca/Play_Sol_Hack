import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PlaceholderEngine } from "../target/types/placeholder_engine"; // Verify this import name matches your IDL
import { assert } from "chai";
import { Keypair, SystemProgram } from "@solana/web3.js";

describe("Hard-Stakes Engine (Physics & Logic)", () => {
  const provider = anchor.AnchorProvider.env();
  anchor.setProvider(provider);

  const program = anchor.workspace.PlaceholderEngine as Program<PlaceholderEngine>;
  
  // We use a generated Keypair for the GameState for simplicity in tests
  // In production, you might use a PDA derived from match_id
  const gameStateKeypair = Keypair.generate();
  const playerOne = (provider.wallet as anchor.Wallet).payer;
  // We need a second wallet for Player 2. We can just generate one.
  const playerTwo = Keypair.generate();

  it("INITIALIZE: Sets up the Dohyo (Ring)", async () => {
    // 1. Init Game (now takes session_key parameter)
    await program.methods
      .initGame(playerOne.publicKey) // Pass playerOne as session key for this test
      .accounts({
        gameState: gameStateKeypair.publicKey,
        payer: playerOne.publicKey,
        systemProgram: SystemProgram.programId,
      })
      .signers([gameStateKeypair])
      .rpc();

    // 2. Fetch State
    const state = await program.account.gameState.fetch(gameStateKeypair.publicKey);

    console.log("   📍 P1 Start:", state.p1Coords.toString());
    
    // Validate Rust logic: [-200, 0]
    assert.equal(state.p1Coords[0].toNumber(), -200); 
    assert.equal(state.p2Coords[0].toNumber(), 200);
    assert.equal(state.mapRadius.toNumber(), 500);
    
    // Safer Enum Check
    assert.ok("waiting" in state.gameStatus); 
  });

  it("PHYSICS: Validates Movement Math", async () => {
    // Rust Logic: NewPos = OldPos + (Input * Speed)
    // Speed is 10.
    // Start P1 X: -200.
    // Input X: 10.
    // Expected: -200 + (10 * 10) = -100.

    const xInput = 10;
    const yInput = 0;

    await program.methods
      .movePlayer(xInput, yInput) // Pass args directly, not an object
      .accounts({
        gameState: gameStateKeypair.publicKey,
        player: playerOne.publicKey, // P1 is the payer/signer here
      })
      .rpc();

    const state = await program.account.gameState.fetch(gameStateKeypair.publicKey);
    console.log("   🚀 P1 New Pos:", state.p1Coords.toString());

    assert.equal(state.p1Coords[0].toNumber(), -100);
  });

  it("MULTIPLAYER: Player 2 can join", async () => {
    // Join with Player 2 (now takes session_key parameter)
    await program.methods
      .joinGame(playerTwo.publicKey) // Pass playerTwo as session key for this test
      .accounts({
        gameState: gameStateKeypair.publicKey,
        playerTwo: playerTwo.publicKey,
      })
      .signers([playerTwo])
      .rpc();

    const state = await program.account.gameState.fetch(gameStateKeypair.publicKey);

    console.log("   👥 Player 2 Joined:", state.playerTwo.toString());

    // Validate P2 is set and game is active
    assert.equal(state.playerTwo.toString(), playerTwo.publicKey.toString());
    assert.ok("active" in state.gameStatus);
  });

  it("MULTIPLAYER: Both players can move independently", async () => {
    // P1 moves right
    await program.methods
      .movePlayer(5, 0)
      .accounts({
        gameState: gameStateKeypair.publicKey,
        player: playerOne.publicKey,
      })
      .rpc();

    // P2 moves left
    await program.methods
      .movePlayer(-3, 2)
      .accounts({
        gameState: gameStateKeypair.publicKey,
        player: playerTwo.publicKey,
      })
      .signers([playerTwo])
      .rpc();

    const state = await program.account.gameState.fetch(gameStateKeypair.publicKey);

    console.log("   📍 P1 Position:", state.p1Coords.toString());
    console.log("   📍 P2 Position:", state.p2Coords.toString());

    // P1 should be at -95 (was -100, moved +5 * 10 = +50)
    assert.equal(state.p1Coords[0].toNumber(), -95);
    // P2 should be at 170 (was 200, moved -3 * 10 = -30)
    assert.equal(state.p2Coords[0].toNumber(), 170);
    assert.equal(state.p2Coords[1].toNumber(), 20);
  });

  it("LOGIC: Detects Ring Out for both players", async () => {
    console.log("   🥊 Pushing P2 out of the ring...");

    // Move P2 towards the edge and out (P2 starts at 170, needs to go beyond 500)
    for(let i=0; i<35; i++) {
        await program.methods
        .movePlayer(10, 0) // Move right towards edge
        .accounts({
            gameState: gameStateKeypair.publicKey,
            player: playerTwo.publicKey,
        })
        .signers([playerTwo])
        .rpc();
    }

    const state = await program.account.gameState.fetch(gameStateKeypair.publicKey);

    // Calculate P2 position
    const p2X = state.p2Coords[0].toNumber();
    const p2Y = state.p2Coords[1].toNumber();
    const distSq = (p2X*p2X) + (p2Y*p2Y);
    const radiusSq = 500*500;

    console.log(`   📏 P2 Distance Squared: ${distSq} vs Radius Squared: ${radiusSq}`);

    if (distSq > radiusSq) {
        console.log("   ✅ P2 Ring Out Detected! P1 Wins!");
        // Check if game status changed to Finished and P1 is winner
        assert.ok("finished" in state.gameStatus);
        assert.equal(state.winner.toString(), playerOne.publicKey.toString());
    } else {
        assert.fail("P2 should be out of bounds but is not.");
    }
  });
});