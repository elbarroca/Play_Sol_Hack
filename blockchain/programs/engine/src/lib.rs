use anchor_lang::prelude::*;
use ephemeral_rollups_sdk::anchor::{delegate, ephemeral, commit};

declare_id!("8HoQnePLqPj4M7PUDzwxKBUBLqjnZ3zDKcn78VFyF5zP"); // Replace with your ID

#[ephemeral] // <--- Activates MagicBlock
#[program]
pub mod placeholder_engine {
    use super::*;

    // 1. Initialize Game State (Delegated)
    pub fn init_game(ctx: Context<InitGame>, game_type: GameType) -> Result<()> {
        let state = &mut ctx.accounts.game_state;
        state.game_type = game_type;
        state.p1_coords = [0, 0];
        state.p2_coords = [50, 0];
        state.p1_rotation = 0;
        state.p2_rotation = 0;
        state.map_bounds = 100; // The "Ring" size
        Ok(())
    }

    // 2. High-Frequency Input Application (Called via Session Key)
    pub fn apply_input(ctx: Context<ApplyInput>, x: i16, y: i16, action: bool) -> Result<()> {
        let state = &mut ctx.accounts.game_state;
        let is_p1 = ctx.accounts.player.key() == state.p1;

        match state.game_type {
            GameType::Tanks => {
                // 1. Get Current Rotation
                let mut rot = if is_p1 { state.p1_rotation } else { state.p2_rotation };

                // 2. Apply Rotation (X Input)
                // Input X (-127 to 127) * TurnSpeed (2)
                rot += (x as i64) * 2;

                // 3. Calculate Forward Vector from Rotation
                // (Simplified trigonometry for hackathon: Lookup table or approx)
                // Real implementation would use: x = cos(rot), z = sin(rot)
                // For now, let's assume raw X/Y input moves the tank like a car (Tank Controls)

                let mut px = if is_p1 { state.p1_coords[0] } else { state.p2_coords[0] };
                let mut py = if is_p1 { state.p1_coords[1] } else { state.p2_coords[1] };

                // Move forward based on Y input relative to Rotation
                // This requires fixed-point trig.
                // SIMPLE HACK FOR MVP: Just move X/Y directly like a top-down shooter.
                px += (x as i64) * 10;
                py += (y as i64) * 10;

                // Save
                if is_p1 { state.p1_coords = [px, py]; state.p1_rotation = rot; }
                else { state.p2_coords = [px, py]; state.p2_rotation = rot; }
            },
            GameType::Sumo => {
                // SUMO LOGIC:
                // Direct X/Y Movement
                if is_p1 {
                    state.p1_coords[0] = state.p1_coords[0].saturating_add(x as i64);
                    state.p1_coords[1] = state.p1_coords[1].saturating_add(y as i64);
                } else {
                    state.p2_coords[0] = state.p2_coords[0].saturating_add(x as i64);
                    state.p2_coords[1] = state.p2_coords[1].saturating_add(y as i64);
                }
            }
        }
        Ok(())
    }
    
    // 3. Check Win Condition
    // If a player is outside map_bounds, the other wins.
    // This triggers the commit back to the Bank.
    pub fn check_win(ctx: Context<CheckWin>) -> Result<()> {
        let state = &ctx.accounts.game_state;
        let radius_sq = state.map_bounds.pow(2);
        
        let p1_dist = state.p1_coords[0].pow(2) + state.p1_coords[1].pow(2);
        let p2_dist = state.p2_coords[0].pow(2) + state.p2_coords[1].pow(2);
        
        if p1_dist > radius_sq {
            msg!("Player 1 fell off! Player 2 Wins.");
            // Here we would call the 'commit' logic
        } else if p2_dist > radius_sq {
            msg!("Player 2 fell off! Player 1 Wins.");
        }
        
        Ok(())
    }
}

#[derive(AnchorSerialize, AnchorDeserialize, Clone, PartialEq, Eq)]
pub enum GameType {
    Tanks,
    Sumo,
}

#[account]
pub struct GameState {
    pub game_type: GameType,
    pub p1: Pubkey,
    pub p2: Pubkey,
    // Unified Coordinates (Used for Tank Position OR Paddle Position)
    pub p1_coords: [i64; 2],
    pub p2_coords: [i64; 2],
    // Unified Rotation (Crucial for Tanks)
    pub p1_rotation: i64,
    pub p2_rotation: i64,
    pub map_bounds: i64,
}

#[derive(Accounts)]
pub struct InitGame<'info> {
    #[account(init, payer = payer, space = 8 + 300)]
    pub game_state: Account<'info, GameState>,
    #[account(mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}

#[derive(Accounts)]
pub struct ApplyInput<'info> {
    #[account(mut)]
    pub game_state: Account<'info, GameState>,
    pub player: Signer<'info>, // Session Key
}

#[derive(Accounts)]
pub struct CheckWin<'info> {
    #[account(mut)]
    pub game_state: Account<'info, GameState>,
}