# NakeCore - Universal Character & TPS System

Bem-vindo ao **NakeCore**, um sistema modular e agnóstico desenvolvido com a filosofia *Solo Dev (KISS e YAGNI)*. 
Este pacote é dividido em duas grandes áreas:
1. **Core:** Scripts universais matemáticos (Vida, Dano, Animação Base) que servem para qualquer personagem (Inimigo, NPC, Player FPS ou TPS).
2. **Templates/TPS:** O controle exclusivo e a câmera de um Jogador em Terceira Pessoa.

---

## 📦 Dependências Necessárias
Antes de arrastar qualquer script para a sua cena, certifique-se de que o seu projeto Unity (Package Manager) possui:
- **Unity New Input System** (`UnityEngine.InputSystem`)
- **Cinemachine** (`Unity.Cinemachine`)

---

## 🎮 Como criar o seu Player (O Efeito Dominó)

Nós desenhamos o sistema para ser **à prova de erros**. Você não precisa lembrar de arrastar 10 scripts diferentes para o seu personagem. Nós usamos o `[RequireComponent]` para criar um **Efeito Dominó**.

Para montar um Player controlável do zero, você **só precisa arrastar UM script** para a sua Cápsula/Modelo 3D:

### Arraste o script `PlayerManager.cs`
Ao arrastar o `PlayerManager` para o seu GameObject, a Unity vai **automaticamente** acoplar os seguintes scripts "filhos" (e nessa exata ordem de comunicação):

1. **`InputReader`**: Automaticamente adicionado. É o responsável por escutar o seu teclado/mouse via *New Input System* e traduzir para comandos.
2. **`PlayerLocomotion`**: Automaticamente adicionado. É o motor de física.
   - *Dominó extra:* O `PlayerLocomotion` automaticamente adiciona um componente `CharacterController` da Unity, garantindo que o boneco não atravesse paredes.
3. **`PlayerAnimationUpdater`**: Automaticamente adicionado. Ele lê a velocidade que o `Locomotion` está gerando e envia para o Animator (fazendo as pernas mexerem).
4. **`TPSCameraAimController`**: Automaticamente adicionado. O script que vai se comunicar com o seu *Cinemachine Virtual Camera* para girar o corpo baseado na visão do mouse.

### Resumo da Comunicação:
O fluxo de dados do seu Player funciona assim:
`InputReader` (Ouve o Teclado) ➔ avisa o ➔ `PlayerManager` (O Cérebro).
O `PlayerManager` ➔ manda o ➔ `PlayerLocomotion` (Andar) e manda o ➔ `TPSCameraAimController` (Olhar).
O `PlayerLocomotion` ➔ muda a velocidade que é lida pelo ➔ `PlayerAnimationUpdater` (Toca a Animação).

---

## ⚔️ Como Adicionar o Combate (O Segundo Dominó)

Se você quer que o seu personagem consiga lutar, o processo é igualmente simples. Você só precisa arrastar mais um script para a mesma cápsula:

### Arraste o script `PlayerCombatManager.cs`
Ao fazer isso, a Unity acionará o "Dominó do Combate" e puxará os módulos do Universal Character System:

1. **`StaminaController`**: Adicionado para gerenciar o fôlego dos ataques.
2. **`CharacterEquipmentManager`**: O módulo universal que gerencia as armas equipadas (nas mãos e costas).
   - *Dominó extra:* Esse script puxa o **`CharacterCombatAnimator`**, que é a ponte universal que faz os golpes da espada se comunicarem com a animação.
   - *Dominó duplo:* E o Animator puxa o **`TPSMeleeVFXController`** para gerenciar os rastros de sangue/efeitos.

*(Nota W.I.P: No futuro, esse mesmo `PlayerCombatManager` será o responsável por puxar também o motor de Armas de Fogo e Arco, centralizando todo tipo de agressão!)*

### Como Adicionar Vida e Dano?
A Vida é **100% Modular e Universal**. Se o seu jogo tiver dano, basta arrastar o script genérico:
- **`CharacterHealthManager.cs`**: Funciona para o Player, para a Caixa de Madeira, para o Boss. Coloque ele, atribua o ScriptableObject de *HealthStats* e pronto. O seu Player agora sangra!
