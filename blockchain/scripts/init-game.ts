import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PlaceholderEngine } from "../target/types/placeholder_engine";
import { Keypair, SystemProgram, PublicKey } from "@solana/web3.js";

async function main() {
  console.log("🎮 Initializing Sumo Game on Devnet...");

  // Set up the connection to Devnet
  const provider = anchor.AnchorProvider.env();
  anchor.setProvider(provider);

  const program = anchor.workspace.PlaceholderEngine as Program<PlaceholderEngine>;

  // Generate a new game state account
  const gameStateKeypair = Keypair.generate();
  const playerOne = (provider.wallet as anchor.Wallet).payer;

  console.log("📍 Game State Account:", gameStateKeypair.publicKey.toString());
  console.log("👤 Player One:", playerOne.publicKey.toString());

  try {
    // Initialize the game
    await program.methods
      .initGame()
      .accounts({
        gameState: gameStateKeypair.publicKey,
        payer: playerOne.publicKey,
        systemProgram: SystemProgram.programId,
      })
      .signers([gameStateKeypair])
      .rpc();

    console.log("✅ Game Initialized Successfully!");

    // Fetch and display the initial state
    const state = await program.account.gameState.fetch(gameStateKeypair.publicKey);

    console.log("🎯 Game State:");
    console.log("   P1 Position:", state.p1Coords.toString());
    console.log("   P2 Position:", state.p2Coords.toString());
    console.log("   Map Radius:", state.mapRadius.toString());
    console.log("   Status:", Object.keys(state.gameStatus)[0]);
    console.log("   Frame Count:", state.frameCount.toString());

    // IMPORTANT: Unity needs this address to connect!
    console.log("\n🚀 COPY THIS ADDRESS TO UNITY:");
    console.log("GameStateAddress:", gameStateKeypair.publicKey.toString());
    console.log("\n💡 Set this in your MagicBlockClient GameStateAddress field");

  } catch (error) {
    console.error("❌ Failed to initialize game:", error);
  }
}

main().catch(console.error);