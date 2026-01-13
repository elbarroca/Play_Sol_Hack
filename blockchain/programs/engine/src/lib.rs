use anchor_lang::prelude::*;

declare_id!("8HoQnePLqPj4M7PUDzwxKBUBLqjnZ3zDKcn78VFyF5zP"); // KEEP YOUR ID

#[program]
pub mod placeholder_engine {
    use super::*;

    pub fn init_game(ctx: Context<InitGame>) -> Result<()> {
        let state = &mut ctx.accounts.game_state;
        state.p1_coords = [-200, 0];
        state.p2_coords = [200, 0];
        state.map_radius = 500;
        state.game_status = GameStatus::Waiting;
        state.frame_count = 0;
        state.player_one = ctx.accounts.payer.key(); // Set P1 to the creator
        msg!("Game Initialized. P1: {:?}", state.player_one);
        Ok(())
    }

    pub fn move_player(ctx: Context<MovePlayer>, x_input: i8, y_input: i8) -> Result<()> {
        let state = &mut ctx.accounts.game_state;

        // Simple Physics
        let speed: i64 = 10;
        state.p1_coords[0] += (x_input as i64) * speed;
        state.p1_coords[1] += (y_input as i64) * speed;

        // Ring Out Logic
        let dist_sq = (state.p1_coords[0] * state.p1_coords[0]) + (state.p1_coords[1] * state.p1_coords[1]);
        if dist_sq > (state.map_radius as i64 * state.map_radius as i64) {
            state.game_status = GameStatus::Finished;
            state.winner = state.player_two; // If P1 falls, P2 wins
            msg!("RING OUT!");
        }

        state.frame_count += 1;
        Ok(())
    }
}

// --- DATA STRUCTURES ---
#[derive(AnchorSerialize, AnchorDeserialize, Clone, Copy, PartialEq, Eq)]
pub enum GameStatus { Waiting, Active, Finished }

#[account]
pub struct GameState {
    pub player_one: Pubkey,
    pub player_two: Option<Pubkey>,
    pub p1_coords: [i64; 2],
    pub p2_coords: [i64; 2],
    pub map_radius: u64,
    pub game_status: GameStatus,
    pub winner: Option<Pubkey>,
    pub frame_count: u64,
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
    pub player: Signer<'info>,
}