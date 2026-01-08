import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PlaceholderBank } from "../target/types/placeholder_bank";
import { assert } from "chai";

describe("Placeholder Bank Tests", () => {
  const provider = anchor.AnchorProvider.env();
  anchor.setProvider(provider);

  const program = anchor.workspace.PlaceholderBank as Program<PlaceholderBank>;
  const player = provider.wallet;

  it("Can create a match and deposit Wager", async () => {
    // 1. Generate Match PDA
    const matchId = new anchor.BN(1);
    const [matchPDA] = anchor.web3.PublicKey.findProgramAddressSync(
      [Buffer.from("match"), player.publicKey.toBuffer(), matchId.toArrayLike(Buffer, "le", 8)],
      program.programId
    );

    // 2. Call CreateMatch
    const wager = new anchor.BN(1000000000); // 1 SOL
    await program.methods
      .createMatch(matchId, wager)
      .accounts({
        matchState: matchPDA,
        player: player.publicKey,
      })
      .rpc();

    // 3. Validation
    const account = await program.account.matchState.fetch(matchPDA);
    assert.ok(account.amount.eq(wager));
    console.log("✅ Match Created with 1 SOL Wager");
  });
});