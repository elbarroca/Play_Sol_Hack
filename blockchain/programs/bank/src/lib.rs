use anchor_lang::prelude::*;
use anchor_lang::system_program;

declare_id!("Fg6PaFpoGXkYsidMpWTK6W2BeZ7FEfcYkg476zPFsLnS"); // Replace with your ID after build

#[program]
pub mod placeholder_bank {
    use super::*;

    // 1. Create a Lobby
    pub fn create_match(ctx: Context<CreateMatch>, match_id: u64, amount: u64) -> Result<()> {
        let match_state = &mut ctx.accounts.match_state;
        match_state.player_one = ctx.accounts.player.key();
        match_state.match_id = match_id;
        match_state.amount = amount;
        match_state.status = MatchStatus::Waiting;

        // Transfer SOL to Vault
        system_program::transfer(
            CpiContext::new(
                ctx.accounts.system_program.to_account_info(),
                system_program::Transfer {
                    from: ctx.accounts.player.to_account_info(),
                    to: ctx.accounts.vault.to_account_info(),
                },
            ),
            amount,
        )?;
        Ok(())
    }

    // 2. Join a Lobby
    pub fn join_match(ctx: Context<JoinMatch>) -> Result<()> {
        let match_state = &mut ctx.accounts.match_state;
        require!(match_state.status == MatchStatus::Waiting, BankError::MatchFull);
        
        match_state.player_two = Some(ctx.accounts.player.key());
        match_state.status = MatchStatus::Active;

        // Transfer SOL to Vault
        system_program::transfer(
            CpiContext::new(
                ctx.accounts.system_program.to_account_info(),
                system_program::Transfer {
                    from: ctx.accounts.player.to_account_info(),
                    to: ctx.accounts.vault.to_account_info(),
                },
            ),
            match_state.amount,
        )?;
        Ok(())
    }

    // 3. Payout (Called after Game Engine finishes)
    pub fn settle_match(ctx: Context<SettleMatch>, winner: Pubkey) -> Result<()> {
        let match_state = &mut ctx.accounts.match_state;
        
        // Validation: Ensure the winner is actually in this match
        require!(
            winner == match_state.player_one || Some(winner) == match_state.player_two,
            BankError::InvalidWinner
        );

        let payout = match_state.amount * 2;

        // Transfer Payout from Vault to Winner
        **ctx.accounts.vault.try_borrow_mut_lamports()? -= payout;
        **ctx.accounts.winner.try_borrow_mut_lamports()? += payout;

        match_state.status = MatchStatus::Completed;
        match_state.winner = Some(winner);
        
        Ok(())
    }
}

#[account]
pub struct MatchState {
    pub player_one: Pubkey,
    pub player_two: Option<Pubkey>,
    pub winner: Option<Pubkey>,
    pub amount: u64,
    pub match_id: u64,
    pub status: MatchStatus,
}

#[derive(AnchorSerialize, AnchorDeserialize, Clone, PartialEq, Eq)]
pub enum MatchStatus {
    Waiting,
    Active,
    Completed,
}

#[derive(Accounts)]
#[instruction(match_id: u64)]
pub struct CreateMatch<'info> {
    #[account(
        init, 
        payer = player, 
        space = 8 + 32 + 33 + 33 + 8 + 8 + 1,
        seeds = [b"match", player.key().as_ref(), match_id.to_le_bytes().as_ref()],
        bump
    )]
    pub match_state: Account<'info, MatchState>,
    #[account(mut)]
    pub player: Signer<'info>,
    #[account(
        mut, 
        seeds = [b"vault", match_state.key().as_ref()], 
        bump
    )]
    pub vault: SystemAccount<'info>,
    pub system_program: Program<'info, System>,
}

#[derive(Accounts)]
pub struct JoinMatch<'info> {
    #[account(mut)]
    pub match_state: Account<'info, MatchState>,
    #[account(mut)]
    pub player: Signer<'info>,
    #[account(mut, seeds = [b"vault", match_state.key().as_ref()], bump)]
    pub vault: SystemAccount<'info>,
    pub system_program: Program<'info, System>,
}

#[derive(Accounts)]
pub struct SettleMatch<'info> {
    #[account(mut)]
    pub match_state: Account<'info, MatchState>,
    /// CHECK: We manually verify this account is the winner
    #[account(mut)]
    pub winner: AccountInfo<'info>,
    #[account(mut, seeds = [b"vault", match_state.key().as_ref()], bump)]
    pub vault: SystemAccount<'info>,
}

#[error_code]
pub enum BankError {
    #[msg("Match is already full.")]
    MatchFull,
    #[msg("The provided winner is not a player in this match.")]
    InvalidWinner,
}