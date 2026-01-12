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
    // 1. Init Game
    await program.methods
      .initGame()
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

  it("LOGIC: Detects Ring Out", async () => {
    // We are at -100. 
    // Edge is 500.
    // We need to move +700 units to be safely out.
    // 700 / (10 speed) = 70 input. 
    // Let's send a massive input to force it (since we clamped input to i8, max is 127).
    
    // Move 6 times with max input (100) to cross the boundary
    // 6 * (100 * 10) = 6000 distance. Plenty.
    
    // Note: In a real game, you'd loop this. In a test, we just want to verify the state transition.
    // Let's just do one big jump if your logic allows it, or a loop.
    
    console.log("   🥊 Pushing P1 out of the ring...");
    
    const pushInput = 120; // i8 max is 127
    
    // Move 5 times
    for(let i=0; i<5; i++) {
        await program.methods
        .movePlayer(pushInput, 0)
        .accounts({
            gameState: gameStateKeypair.publicKey,
            player: playerOne.publicKey, 
        })
        .rpc();
    }

    const state = await program.account.gameState.fetch(gameStateKeypair.publicKey);
    
    // Calculate P1 position
    const p1X = state.p1Coords[0].toNumber();
    const p1Y = state.p1Coords[1].toNumber();
    const distSq = (p1X*p1X) + (p1Y*p1Y);
    const radiusSq = 500*500;

    console.log(`   📏 Distance Squared: ${distSq} vs Radius Squared: ${radiusSq}`);

    if (distSq > radiusSq) {
        console.log("   ✅ Ring Out Detected!");
        // Check if game status changed to Finished
        assert.ok("finished" in state.gameStatus);
    } else {
        assert.fail("Player should be out of bounds but is not.");
    }
  });
});