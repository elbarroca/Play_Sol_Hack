# 💰 Hard-Stakes: Bank & Escrow Module

Este módulo é responsável por toda a logística financeira do Hard-Stakes. Ele garante que as apostas em SOL sejam custodiadas com segurança e distribuídas corretamente após o fim da partida.

## 🏗️ Arquitetura do Escrow

O sistema utiliza **PDAs (Program Derived Addresses)** para criar cofres (vaults) isolados por partida. 

1. **Match State PDA**: Armazena os metadados (quem joga, quanto apostaram, status).
2. **Vault PDA**: Uma conta de sistema controlada pelo programa que guarda o SOL.

### Fluxo de Dinheiro
1. **P1 (Criador)**: Chama `create_match`, define o valor (ex: 0.1 SOL) e envia para o Vault.
2. **P2 (Oponente)**: Chama `join_match`, deposita o valor idêntico no mesmo Vault.
3. **Settlement**: Quando o jogo termina, a função `settle_match` é chamada:
   - **98%** do pote vai para o vencedor.
   - **2%** do pote vai para a carteira da "Casa" (Admin Fee).

## 🔒 Segurança e Validação

### Proteções no Código (Rust/Anchor)
- **Constraint de Endereço**: Apenas o administrador definido na criação pode receber a taxa.
- **Integridade do Vencedor**: O código verifica se o vencedor passado na transação é realmente o Player 1 ou o Player 2 daquela partida específica.
- **Estado Bloqueado**: Partidas concluídas (`Completed`) não podem ser pagas duas vezes.
- **Safety Checks**: Utilização de `SystemAccount` e `UncheckedAccount` com validação de sementes (`seeds`) e `bump`.

## ✅ Validação (Testes)

O módulo foi validado via **Anchor Integration Tests** (TypeScript) com o seguinte fluxo:

| Teste | Descrição | Status |
|-------|-----------|--------|
| `Prepara Player 2` | Garante que o oponente tem saldo para apostar. | ✅ Pass |
| `Create Match` | Valida criação da PDA e depósito inicial de 0.1 SOL. | ✅ Pass |
| `Join Match` | Valida entrada do P2 e bloqueio de 0.2 SOL totais. | ✅ Pass |
| `Settle Match` | Valida distribuição: Winner (0.196 SOL) + House (0.004 SOL). | ✅ Pass |

### Como rodar os testes
```bash
cd blockchain
anchor test
