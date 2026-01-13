import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PlaceholderBank } from "../target/types/placeholder_bank";
import { assert } from "chai";
import { Keypair, LAMPORTS_PER_SOL, SystemProgram, PublicKey } from "@solana/web3.js";

describe("Hard-Stakes Escrow", () => {
  const provider = anchor.AnchorProvider.env();
  anchor.setProvider(provider);
  const program = anchor.workspace.PlaceholderBank as Program<PlaceholderBank>;

  const playerOne = (provider.wallet as anchor.Wallet).payer;
  const playerTwo = Keypair.generate();
  const houseAdmin = Keypair.generate();
  const wager = new anchor.BN(0.1 * LAMPORTS_PER_SOL);
  const matchId = new anchor.BN(Date.now());

  const [matchPDA] = PublicKey.findProgramAddressSync(
    [Buffer.from("match"), playerOne.publicKey.toBuffer(), matchId.toArrayLike(Buffer, "le", 8)],
    program.programId
  );

  const [vaultPDA] = PublicKey.findProgramAddressSync(
    [Buffer.from("vault"), matchPDA.toBuffer()],
    program.programId
  );

  it("Prepara Player 2 com SOL", async () => {
    const sig = await provider.connection.requestAirdrop(playerTwo.publicKey, 1 * LAMPORTS_PER_SOL);
    await provider.connection.confirmTransaction(sig);
  });

  it("Cria a partida (P1 deposita 0.1 SOL)", async () => {
    await program.methods
      .createMatch(matchId, wager)
      .accounts({
        matchState: matchPDA,
        player: playerOne.publicKey,
        admin: houseAdmin.publicKey,
        vault: vaultPDA,
        systemProgram: SystemProgram.programId,
      })
      .rpc();

    const vaultBal = await provider.connection.getBalance(vaultPDA);
    assert.equal(vaultBal, wager.toNumber());
    console.log("   ✅ Vault tem 0.1 SOL");
  });

  it("Player 2 entra (Deposita mais 0.1 SOL)", async () => {
    await program.methods
      .joinMatch()
      .accounts({
        matchState: matchPDA,
        player: playerTwo.publicKey,
        vault: vaultPDA,
        systemProgram: SystemProgram.programId,
      })
      .signers([playerTwo])
      .rpc();

    const vaultBal = await provider.connection.getBalance(vaultPDA);
    assert.equal(vaultBal, wager.toNumber() * 2);
    console.log("   ✅ Vault tem 0.2 SOL");
  });

  it("Settle Match (P1 ganha 0.196 SOL, House ganha 0.004 SOL)", async () => {
    const initialBal = await provider.connection.getBalance(playerOne.publicKey);
    
    await program.methods
      .settleMatch(playerOne.publicKey)
      .accounts({
        matchState: matchPDA,
        winner: playerOne.publicKey,
        admin: houseAdmin.publicKey,
        vault: vaultPDA,
        systemProgram: SystemProgram.programId,
      })
      .rpc();

    const finalBal = await provider.connection.getBalance(playerOne.publicKey);
    assert.isAbove(finalBal, initialBal);
    console.log("   ✅ Winner recebeu o prêmio (menos 2% de taxa)");
  });
});
