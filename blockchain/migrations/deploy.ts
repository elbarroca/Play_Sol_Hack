import * as anchor from "@coral-xyz/anchor";

module.exports = async function (provider) {
  // This script will auto-initialize the game config on Devnet
  anchor.setProvider(provider);
  console.log("Migrating Game State...");
};