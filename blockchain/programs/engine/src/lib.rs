use anchor_lang::prelude::*;

declare_id!("8HoQnePLqPj4M7PUDzwxKBUBLqjnZ3zDKcn78VFyF5zP"); // Use YOUR Program ID after 'anchor keys list'

#[program]
pub mod placeholder_engine {
    use super::*;

    pub fn init_game(ctx: Context<InitGame>) -> Result<()> {
        let state = &mut ctx.accounts.game_state;

        // Setup Sumo Positions (Fixed Point: Divide by 100 in Unity)
        state.p1_coords = [-200, 0]; // x = -2.0m
        state.p2_coords = [200, 0];  // x = +2.0m

        state.map_radius = 500;      // 5.0m Ring
        state.game_status = GameStatus::Waiting;
        state.frame_count = 0;

        msg!("Sumo Game Initialized at P1: {:?}, P2: {:?}", state.p1_coords, state.p2_coords);
        Ok(())
    }
}

// --- DATA STRUCTURES ---

#[derive(AnchorSerialize, AnchorDeserialize, Clone, Copy, PartialEq, Eq)]
pub enum GameStatus {
    Waiting,
    Active,
    Finished
}

#[account]
pub struct GameState {
    pub p1_coords: [i64; 2], // [x, z] - No Y needed for Sumo on flat ground
    pub p2_coords: [i64; 2],
    pub map_radius: u64,     // The "Dohyo" ring size
    pub game_status: GameStatus,
    pub frame_count: u64,    // For lag compensation logic later
}

#[derive(Accounts)]
pub struct InitGame<'info> {
    #[account(init, payer = payer, space = 8 + 16 + 16 + 8 + 2 + 8)]
    pub game_state: Account<'info, GameState>,
    #[account(mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}