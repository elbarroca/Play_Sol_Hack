use anchor_lang::prelude::*;
use ephemeral_rollups_sdk::anchor::{delegate, ephemeral, commit};

declare_id!("8HoQnePLqPj4M7PUDzwxKBUBLqjnZ3zDKcn78VFyF5zP"); // Replace with your ID

#[ephemeral] // <--- Activates MagicBlock
#[program]
pub mod placeholder_engine {
    use super::*;

    // 1. Initialize Game State (Delegated)
    pub fn init_game(ctx: Context<InitGame>) -> Result<()> {
        let state = &mut ctx.accounts.game_state;
        state.p1_coords = [0, 0];   // Center
        state.p2_coords = [20, 0];  // Slightly Right
        state.map_bounds = 100;     // The "Ring" size
        state.frame = 0;
        Ok(())
    }

    // 2. High-Frequency Move (Called via Session Key)
    pub fn move_player(ctx: Context<MovePlayer>, x: i16, y: i16) -> Result<()> {
        let state = &mut ctx.accounts.game_state;
        
        // Identify Player
        let is_p1 = ctx.accounts.player.key() == state.p1;
        
        // Update Coordinates (Simple Physics)
        if is_p1 {
            state.p1_coords[0] = state.p1_coords[0].saturating_add(x as i64);
            state.p1_coords[1] = state.p1_coords[1].saturating_add(y as i64);
        } else {
            state.p2_coords[0] = state.p2_coords[0].saturating_add(x as i64);
            state.p2_coords[1] = state.p2_coords[1].saturating_add(y as i64);
        }
        
        state.frame += 1;
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

#[account]
pub struct GameState {
    pub p1: Pubkey,
    pub p2: Pubkey,
    pub p1_coords: [i64; 2],
    pub p2_coords: [i64; 2],
    pub map_bounds: i64,
    pub frame: u64,
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
pub struct MovePlayer<'info> {
    #[account(mut)]
    pub game_state: Account<'info, GameState>,
    pub player: Signer<'info>, // Session Key
}

#[derive(Accounts)]
pub struct CheckWin<'info> {
    #[account(mut)]
    pub game_state: Account<'info, GameState>,
}