use anchor_lang::prelude::*;

declare_id!("2JfW8D59eJ1myVbqpU8BBLxkWp3Bhwf6yjY8HfuqBSHv"); // KEEP YOUR ID

#[program]
pub mod placeholder_engine {
    use super::*;

    pub fn init_game(ctx: Context<InitGame>, session_key: Pubkey) -> Result<()> {
        let state = &mut ctx.accounts.game_state;

        // Store the Main Wallet for payouts (production feature)
        // state.player_one_principal = ctx.accounts.payer.key();

        state.player_one = session_key; // <--- VITAL: Authorize Session Key for movements
        state.p1_coords = [-200, 0];
        state.p2_coords = [200, 0];
        state.map_radius = 500;
        state.game_status = GameStatus::Waiting;
        state.frame_count = 0;
        msg!("Game Init. Session Key Authority: {:?}", state.player_one);
        Ok(())
    }

    pub fn join_game(ctx: Context<JoinGame>, session_key: Pubkey) -> Result<()> {
        let state = &mut ctx.accounts.game_state;
        require!(state.game_status == GameStatus::Waiting, GameError::GameNotOpen);
        require!(state.player_two.is_none(), GameError::GameFull);

        // Store the Main Wallet for payouts (production feature)
        // state.player_two_principal = ctx.accounts.player_two.key();

        state.player_two = Some(session_key); // <--- VITAL: Authorize Session Key for movements
        state.game_status = GameStatus::Active; // Start the game!
        msg!("Player 2 Joined. Session Key: {:?}", state.player_two);
        Ok(())
    }

    pub fn move_player(ctx: Context<MovePlayer>, x_input: i8, y_input: i8) -> Result<()> {
        let state = &mut ctx.accounts.game_state;
        let signer = ctx.accounts.player.key();

        // 1. Identify Who is Moving
        let is_p1 = signer == state.player_one;
        let is_p2 = state.player_two.is_some() && signer == state.player_two.unwrap();

        require!(state.game_status == GameStatus::Active, GameError::GameNotActive);

        // 2. Apply Physics (with overflow protection)
        let speed: i64 = 10;
        if is_p1 {
            state.p1_coords[0] = state.p1_coords[0].saturating_add((x_input as i64) * speed);
            state.p1_coords[1] = state.p1_coords[1].saturating_add((y_input as i64) * speed);
        } else if is_p2 {
            state.p2_coords[0] = state.p2_coords[0].saturating_add((x_input as i64) * speed);
            state.p2_coords[1] = state.p2_coords[1].saturating_add((y_input as i64) * speed);
        }

        // 3. Win Condition (Ring Out)
        // Check P1
        let p1_dist = (state.p1_coords[0].pow(2) + state.p1_coords[1].pow(2));
        let radius_sq = state.map_radius.pow(2) as i64;

        if p1_dist > radius_sq {
            state.game_status = GameStatus::Finished;
            state.winner = state.player_two; // P1 fell, P2 wins
            msg!("P1 Ring Out! Winner: P2");
        }
        // Check P2
        else if state.player_two.is_some() {
            let p2_dist = (state.p2_coords[0].pow(2) + state.p2_coords[1].pow(2));
            if p2_dist > radius_sq {
                state.game_status = GameStatus::Finished;
                state.winner = Some(state.player_one); // P2 fell, P1 wins
                msg!("P2 Ring Out! Winner: P1");
            }
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
pub struct JoinGame<'info> {
    #[account(mut)]
    pub game_state: Account<'info, GameState>,
    #[account(mut)]
    pub player_two: Signer<'info>,
}

#[derive(Accounts)]
pub struct MovePlayer<'info> {
    #[account(mut)]
    pub game_state: Account<'info, GameState>,
    pub player: Signer<'info>,
}

#[error_code]
pub enum GameError {
    #[msg("Game is full")] GameFull,
    #[msg("Game not in waiting state")] GameNotOpen,
    #[msg("Game is not active")] GameNotActive,
}
