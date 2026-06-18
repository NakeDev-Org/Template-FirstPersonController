# Relatório de Arquitetura e Polimento (NakeDev-Studio)

Abaixo está o raio-X completo da sua arquitetura atual, focado na pragmática de "Solo Dev" para podermos escalar rápido e sem dores de cabeça.

## 1. Defeitos de Responsividade e Polimento
**O que precisa de refino (Não Finalizado):**
- ~~**Momentum Aéreo (Pulo):** A física de pulo está engessada. Ao soltar o analógico no ar ou parar de correr, o script de Locomotion zera o `targetSpeed` e freia o personagem no ar. O pulo precisa preservar a inércia do chão (ignorar o Lerp de atrito aéreo).~~ *(Resolvido!)*
- **Buffer de Inputs (Combos):** Seus ataques ainda funcionam numa estrutura de `SetActionBlocked`. Falta uma fila de buffer ("Input Queue") para que pressionar "Ataque" milissegundos antes da animação acabar já engatilhe o próximo golpe, garantindo a fluidez de um Hack & Slash.
- **Cinemachine Clipping:** Precisamos garantir que a Câmera tenha o módulo de colisão ajustado para o raio do jogador, impedindo que ela entre em geometria complexa e mate o over-the-shoulder.

**O que está sólido (Finalizado):**
- **Input System (InputReader):** Totalmente orientado a eventos e altamente performático. 
- **Sistema de Lock-On Híbrido:** Transições suaves entre Free-Look e Target Camera agora operam perfeitamente integradas sem snaps ou gambiarras.
- **Gerenciamento de Estado (PlayerManager):** State Machine pragmática, fácil de debugar e de expandir.

---

## 2. O Que é Modular vs Não Modular?

### ❌ Não Modular (Muito atrelado ao "Player")
A pasta `NakeD.Template.TPS\Player\Modular` tem um problema de acoplamento conceitual:
- **`PlayerLocomotion` e `PlayerManager`:** A lógica de andar e animar é excelente, mas os scripts assumem que sempre haverá um `InputReader` e uma Câmera (`Camera.main`). 
- **`TargetingSystem`:** Atualmente, ele usa o *Forward da Câmera* para calcular se o inimigo está visível e qual o melhor alvo. Isso quebra qualquer possibilidade de um Inimigo usar esse mesmo script para procurar o Player.

### ✅ Modular (Brilhante)
A pasta `NakeD.CombatCore` está perfeita.
- **`CharacterEquipmentManager`:** Ao usar "Character", você acertou na veia. A lógica de empunhar espadas ou arcos não faz distinção se quem está segurando é o Player ou um Inimigo.
- **Sistema de Dano (`NakeD.DamageSystem`):** Totalmente agnóstico, lida apenas com Hitboxes e Health.

---

## 3. Reaproveitamento para Inimigos (O Que Posso Usar?)

**🟢 Você PODE e DEVE jogar nos Inimigos hoje:**
- Todo o pacote `NakeD.CombatCore` (Equipamentos e Stats).
- Todo o `NakeD.DamageSystem`. Seus inimigos vão receber dano, dar parry e morrer usando a mesmíssima estrutura do Player.
- Modelos de `Animator` (A lógica da Animator Controller de transição entre combates).

**🔴 Você NÃO CONSEGUE reaproveitar hoje:**
- **`TargetingSystem`:** Um inimigo não tem câmera. Precisaremos refatorar o `TargetingSystem` para usar um `Transform visionPivot` (que no player é a Câmera, e no inimigo são os olhos/corpo).
- **`PlayerLocomotion`:** Para que os NPCs andem usando a mesma física, precisamos extrair a leitura de Input de dentro dele. A solução KISS aqui é criar uma interface `IMovementInput` (Onde o Player passa o Joystick, e o Inimigo passa o NavMeshAgent). 

---

## Próximo Passo Sugerido (Quest)
1. Extrair a dependência da `Camera.main` do `TargetingSystem` (permitindo que Inimigos usem o sistema de visão).
2. ~~Arrumar a inércia do pulo para resolver a sensação de "trava" no ar que você reportou mais cedo.~~ *(Resolvido!)*
