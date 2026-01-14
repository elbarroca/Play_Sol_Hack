use anchor_lang::prelude::*;
use anchor_lang::system_program;

declare_id!("J5juVUkDeUwb3rXT1saab2q87aXyBjzmuAZ7dhMCfbka");

#[program]
pub mod placeholder_bank {
    use super::*;

    pub fn create_match(ctx: Context<CreateMatch>, match_id: u64, amount: u64) -> Result<()> {
        require!(amount > 0, BankError::InvalidAmount);

        let match_state = &mut ctx.accounts.match_state;
        match_state.player_one = ctx.accounts.player.key();
        match_state.player_two = None;
        match_state.winner = None;
        match_state.admin = ctx.accounts.admin.key();
        match_state.match_id = match_id;
        match_state.amount = amount;
        match_state.status = MatchStatus::Waiting;

        // Transferência inicial de P1 para o Vault
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

    pub fn join_match(ctx: Context<JoinMatch>) -> Result<()> {
        let match_state = &mut ctx.accounts.match_state;

        require!(match_state.status == MatchStatus::Waiting, BankError::MatchFull);
        require!(
            ctx.accounts.player.key() != match_state.player_one,
            BankError::SamePlayer
        );

        match_state.player_two = Some(ctx.accounts.player.key());
        match_state.status = MatchStatus::Active;

        // Transferência de P2 para o Vault
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

    pub fn settle_match(ctx: Context<SettleMatch>, winner: Pubkey) -> Result<()> {
        let match_state = &mut ctx.accounts.match_state;

        require!(match_state.status == MatchStatus::Active, BankError::InvalidState);
        require!(
            winner == match_state.player_one || Some(winner) == match_state.player_two,
            BankError::InvalidWinner
        );

        let total_pot = match_state.amount.checked_mul(2).ok_or(BankError::MathOverflow)?;
        let fee = total_pot.checked_mul(200).ok_or(BankError::MathOverflow)?.checked_div(10_000).ok_or(BankError::MathOverflow)?;
        let payout = total_pot.checked_sub(fee).ok_or(BankError::MathOverflow)?;

        // Seeds para assinar como Vault PDA
        let match_state_key = match_state.key();
        let seeds = &[
            b"vault",
            match_state_key.as_ref(),
            &[ctx.bumps.vault],
        ];
        let signer_seeds = &[&seeds[..]];

        // 1) Payout -> Vencedor
        system_program::transfer(
            CpiContext::new_with_signer(
                ctx.accounts.system_program.to_account_info(),
                system_program::Transfer {
                    from: ctx.accounts.vault.to_account_info(),
                    to: ctx.accounts.winner.to_account_info(),
                },
                signer_seeds,
            ),
            payout,
        )?;

        // 2) Fee -> Casa (Admin)
        system_program::transfer(
            CpiContext::new_with_signer(
                ctx.accounts.system_program.to_account_info(),
                system_program::Transfer {
                    from: ctx.accounts.vault.to_account_info(),
                    to: ctx.accounts.admin.to_account_info(),
                },
                signer_seeds,
            ),
            fee,
        )?;

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
    pub admin: Pubkey,
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
        space = 8 + 32 + 33 + 33 + 32 + 8 + 8 + 1,
        seeds = [b"match", player.key().as_ref(), match_id.to_le_bytes().as_ref()],
        bump
    )]
    pub match_state: Account<'info, MatchState>,

    #[account(mut)]
    pub player: Signer<'info>,

    /// CHECK: Apenas uma conta para receber taxas
    pub admin: UncheckedAccount<'info>,

    /// CHECK: Este é um PDA que apenas guarda SOL, não precisa de espaço de dados
    #[account(
        mut,
        seeds = [b"vault", match_state.key().as_ref()],
        bump
    )]
    pub vault: UncheckedAccount<'info>,

    pub system_program: Program<'info, System>,
}

#[derive(Accounts)]
pub struct JoinMatch<'info> {
    #[account(mut)]
    pub match_state: Account<'info, MatchState>,
    #[account(mut)]
    pub player: Signer<'info>,
    /// CHECK: Validado via seeds
    #[account(
        mut,
        seeds = [b"vault", match_state.key().as_ref()],
        bump
    )]
    pub vault: UncheckedAccount<'info>,
    pub system_program: Program<'info, System>,
}

#[derive(Accounts)]
pub struct SettleMatch<'info> {
    #[account(mut)]
    pub match_state: Account<'info, MatchState>,
    /// CHECK: Conta do vencedor (recebe SOL)
    #[account(mut)]
    pub winner: UncheckedAccount<'info>,
    /// CHECK: Conta da casa (recebe taxas)
    #[account(mut, address = match_state.admin)]
    pub admin: UncheckedAccount<'info>,
    /// CHECK: Validado via seeds
    #[account(
        mut,
        seeds = [b"vault", match_state.key().as_ref()],
        bump
    )]
    pub vault: UncheckedAccount<'info>,
    pub system_program: Program<'info, System>,
}

#[error_code]
pub enum BankError {
    #[msg("A partida está cheia.")]
    MatchFull,
    #[msg("Estado da partida inválido.")]
    InvalidState,
    #[msg("O vencedor não pertence a esta partida.")]
    InvalidWinner,
    #[msg("O valor deve ser maior que zero.")]
    InvalidAmount,
    #[msg("Não podes jogar contra ti mesmo.")]
    SamePlayer,
    #[msg("Erro matemático (Overflow).")]
    MathOverflow,
}
