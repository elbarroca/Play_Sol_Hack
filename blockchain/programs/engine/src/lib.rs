use anchor_lang::prelude::*;

declare_id!("8HoQnePLqPj4M7PUDzwxKBUBLqjnZ3zDKcn78VFyF5zP"); // ENSURE THIS MATCHES YOUR KEY

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
        // IMPORTANT: Assign the signer as Player One so they can move!
        state.player_one = ctx.accounts.payer.key();
        Ok(())
    }

    // 👇 THIS WAS MISSING. WITHOUT THIS, NOTHING MOVES.
    pub fn move_player(ctx: Context<MovePlayer>, x_input: i8, y_input: i8) -> Result<()> {
        let state = &mut ctx.accounts.game_state;
        
        // Simple Physics: New = Old + (Input * Speed)
        let speed: i64 = 10; 
        state.p1_coords[0] += (x_input as i64) * speed;
        state.p1_coords[1] += (y_input as i64) * speed;
        
        state.frame_count += 1;
        msg!("Moved P1 to: {:?}", state.p1_coords);
        Ok(())
    }
}

#[derive(AnchorSerialize, AnchorDeserialize, Clone, Copy, PartialEq, Eq)]
pub enum GameStatus { Waiting, Active, Finished }

#[account]
pub struct GameState {
    pub player_one: Pubkey, // Added Identity
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
    #[account(init, payer = payer, space = 8 + 300)] // Bumped space for safety
    pub game_state: Account<'info, GameState>,
    #[account(mut)]
    pub payer: Signer<'info>,
    pub system_program: Program<'info, System>,
}

#[derive(Accounts)]
pub struct MovePlayer<'info> {
    #[account(mut)]
    pub game_state: Account<'info, GameState>,
    pub player: Signer<'info>, // The Session Key signs this!
}