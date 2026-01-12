# 🎮 Hard-Stakes: Game Engine Module (MagicBlock)

Enquanto o Módulo Bancário (Bank) protege o dinheiro com cofres e regras rígidas, o Engine Module é o cérebro ágil do jogo. Ele foi desenhado para rodar sobre a infraestrutura de Ephemeral Rollups da MagicBlock, permitindo uma jogabilidade fluida (sub-50ms) que seria impossível na Mainnet da Solana padrão.

## 🏎️ Arquitetura: Ephemeral Rollups

Este não é um Smart Contract comum. Ele opera em dois modos:

### Modo "Base" (Solana L1):
- Cria a sessão de jogo (`init_game`).
- Define quem são os jogadores e o estado inicial.
- Resolve o resultado final (`settle_match`).

### Modo "Delegado" (Ephemeral Validator):
- Durante a partida, a conta de estado (GameState) é delegada para um validador de alta performance (MagicBlock).
- Os inputs de movimento (`move_player`) são processados instantaneamente sem tempos de confirmação de bloco (400ms -> ~20ms).
- Transações de jogo são "Gas-less" ou de custo negligenciável.

## 🤼 Física do Sumo (On-Chain)

O Unity renderiza os gráficos bonitos, mas a verdade absoluta da posição dos jogadores reside neste programa Rust.

### 1. Matemática de Ponto Fixo (Fixed Point)
A Solana e a BPF (Berkeley Packet Filter) não lidam bem com números flutuantes (float). Para precisão no Unity, usamos Inteiros Escalados.

**Escala:** 100

**Exemplo:**
- Unity (Visual): 2.55 metros.
- Blockchain (Lógico): 255 unidades.

### 2. Inputs de Hardware (PSG1)
A função `move_player` aceita inputs diretos do Joystick do Play Solana Gen 1.

- Input X/Y: `i8` (Inteiro de 8 bits, varia de -127 a 127).
- Velocidade: Constante definida no contrato (ex: 10 unidades/tick).

### 3. A Regra do "Ring Out" (Vitória)
A cada movimento, o contrato verifica a distância do jogador até o centro (0,0). Se a distância for maior que o raio do "Dohyo" (Ringue), o estado muda imediatamente.

```rust
// Lógica Simplificada (Pythagoras sem raiz quadrada para performance)
let dist_sq = (x * x) + (y * y);
let radius_sq = map_radius * map_radius;

if dist_sq > radius_sq {
    game_status = Finished;
    winner = Oponente;
}
```

## 🔒 Validação e Segurança

O Engine implementa proteções contra trapaças comuns em jogos P2P:

- **Autoridade de Servidor**: O cliente envia "intenção de movimento" (Input), o servidor calcula a "nova posição". Teletransporte é impossível.
- **Verificação de Estado**: Não é possível mover se o jogo estiver `Waiting` ou `Finished`.
- **Identidade**: Apenas as chaves públicas registradas no `init_game` (ou Session Keys delegadas) podem assinar movimentos.

## ✅ Guia de Validação (Testes)

Para garantir que o motor está pronto para o Unity, executamos o script `tests/engine.ts`.

| Teste | O que valida? | Expectativa |
|-------|---------------|-------------|
| Initialize | Setup do Tabuleiro | P1 em [-200, 0], P2 em [200, 0], Status Waiting. |
| Physics Move | Input do Joystick | Enviar x=10. Posição deve atualizar para -190 (Matemática correta). |
| Ring Out | Condição de Vitória | Mover P1 para fora do raio 500. Status deve mudar para Finished. |

### Como rodar a validação

```bash
cd blockchain
# Garante que o build está atualizado
anchor build

# Roda a simulação física local
anchor test --run tests/engine.ts
```

## 📝 Notas de Integração (Unity Client)

- **Desserialização**: Ao ler a conta GameState no C#, lembre-se de dividir as coordenadas por 100.0f para obter a posição no mundo Unity.
- **Predição**: O cliente deve mover o jogador visualmente antes da resposta da blockchain para suavidade, mas deve corrigir (snap) se a resposta da blockchain diferir (Reconciliação).

## 🚀 Deploy to Devnet

Now that the code is fixed, push it to the network.

**Terminal:** `cd blockchain`  
**Build:** `anchor build`  
**Deploy:** `anchor deploy --provider.cluster devnet`

(Copy the Program Id output. Ensure it matches your code. If it changed, update lib.rs and MagicBlockClient.cs).

## 🎮 The Validation Ritual (How to Test)

Now we connect the pieces.

### Generate the Game Address

Run your initialization script (ensure blockchain/scripts/init-game.ts uses the new struct structure, or just use the test command):

```bash
anchor test --skip-deploy --run tests/engine.ts
```

Look at the logs for "Game State Address" or "GameState Public Key".

### Configure Unity

Go to Inspector -> [NetworkManager].  
Paste that address into Game State Address.

### Fuel the Session

Since we don't have the "Transfer SOL" button yet, manually send 0.01 SOL to the Session Key Address you see in the Console log (from your Phantom wallet or CLI `solana transfer <KEY> 0.01`).

### Play

Press Play.  
Move the Joystick.

### Validation

If the ball moves in the Unity Scene View, IT WORKS.  

**Flow:** Input -> Bridge -> MagicBlock Client -> Solana Devnet -> Rust Logic -> Solana State -> MagicBlock Client -> NetworkSumo -> Transform.position.
