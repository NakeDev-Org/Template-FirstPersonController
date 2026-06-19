# Survival Horror Refactoring Roadmap

Este documento serve como nosso guia definitivo para transformar o framework TPS genérico em um jogo focado no estilo Survival Horror clássico/moderno (focado em peso, tensão e tiroteio sobre o ombro).

## ✅ Phase 1: Locomotion & Physics (Concluído)
- [x] Remover movimentação `FreeDirectional` (Livre) e focar 100% no `CameraStrafe` (Andar travado na câmera).
- [x] Adaptar `PlayerLocomotion.cs` para rotacionar o jogador apenas quando houver input de movimento ou mira.
- [x] Configurar o `PlayerAnimationUpdater.cs` para enviar `Horizontal` (X) e `Vertical` (Y) reais para pilotar a BlendTree 2D de Strafe.
- [x] **YAGNI / Limpeza:** Remover completamente o conceito de **Pulo** do jogo (`ProcessJump`, inputs, hashes no Animator e variáveis no `PlayerLocomotionStats`), focando no peso realista.

## ✅ Phase 1.1: Locomotion Polish (Concluído)
- [x] Corrigir BlendTree 2D: Mapear valores enviados ao Animator para uma escala exata (0.5 max para Walk, 1.0 max para Jog), abandonando a injeção da velocidade física direta (`CurrentSpeed`).
- [x] Suavizar entrada do analógico para evitar "snaps" na animação.

## ✅ Phase 2: Purging the Combo System (Concluído)
- [x] Excluir a pasta `NakeD.ComboFramework` inteira.
- [x] Limpar `PlayerManager.cs` / `PlayerCombat.cs` para desvincular o sistema de grafos de combo.
- [x] Manter o corpo-a-corpo restrito a apenas 1 hit defensivo rápido.

## ✅ Phase 2.1: Purging RPG Elements from Melee (Concluído)
- [x] Simplificar o sistema de `MeleeWeaponStats`. Remover variáveis complexas de RPG (Dano de Fogo, Luz, Sangramento, etc).
- [x] Focar em um sistema de dano simples, físico e direto (Raw Damage + Impacto).

## ✅ Phase 3: Ranged Combat (Concluído)
- [x] Remover ou deletar componentes específicos para Arco e Flecha (Física balística complexa de projéteis lentos) do `NakeD.RangedFramework`.
- [x] Refinar os scripts de armas de fogo (`RangedWeapon.cs`) para operar em modo **Hitscan** puro (Raycast saindo do centro da tela).
- [x] Validar sistemas de consumo de munição (Pente) e Tempo de Recarga (Reload).

## ✅ Phase 4: Camera & Aiming (Concluído)
- [x] Eliminar o `TargetingSystem.cs` atual (que usa `Physics.OverlapSphere` para travar a mira automaticamente em inimigos como em Dark Souls).
- [x] Substituir por um `AimController` focado em Mira Sobre o Ombro (OTS - Over The Shoulder).
- [x] Fazer com que segurar a mira ("LT" / "Right Click") dê um zoom suave no FOV da Cinemachine, ative a retícula central na tela e trave a rotação do personagem perfeitamente na câmera.

## ⏳ Phase 5: Dogfooding & Extensibility Architecture (NakeDev Framework)
*Refatoração profunda para garantir que o projeto final não modifique a Framework. Prioridade absoluta é usar `protected virtual`. Caso seja um método usado por outras classes, usaremos `public virtual`.*

### 5.1 Sistema de Dano & Combate Base (`NakeD.CombatCore`) - ✅ (Concluído)
- [x] **CharacterHealthManager.cs:** Converter `ApplyDamage`, `Start` para `virtual`. Criar `protected virtual void Die()`.
- [x] **CharacterEquipmentManager.cs:** Converter `WeaponEquip`, `Attack`, `ToggleCombatMode`, `StartBlock`, `StopBlock`, `UnequipWeapon`, `ResetWeapon`, `Unequip` para `virtual`.
- [x] **CharacterCombatAnimator.cs:** Converter `EquipWeapon`, `Attack`, `PerformAttack`, `ResetAttack`, `EnterCombatMode`, `ExitCombatMode`, `SetWeaponInHand`, `SetBlocking`, `Hit` para `virtual`.

### 5.2 Sistema de Armas de Fogo (`NakeD.RangedFramework`) - ✅ (Concluído)
- [x] **CharacterRangedCombat.cs:** Converter `HandleAimStarted`, `HandleAimCanceled`, `HandleAttackPressed`, `TryShoot`, `PerformShoot`, `EquipRangedWeapon`, `UnequipRangedWeapon`, `CanAimOrShoot` para `virtual`.
- [x] **CharacterAimController.cs:** Converter `HandleAimStarted`, `HandleAimCanceled` para `virtual`.

### 5.3 Controle e Estado do Jogador (`NakeD.Template.TPS.Player`) - ✅ (Concluído)
- [x] **PlayerAnimationUpdater.cs:** Converter `Update`, `UpdateAnimatorParameters` para `virtual`.
- [x] **PlayerManager.cs:** Converter `Awake`, `Start` para `protected virtual`.
- [x] **InputReader.cs:** Converter Callbacks (`OnMove`, `OnJump`, `OnAim`, etc) para `protected virtual`.

### 5.4 Sistema de Interação (`NakeD.InteractionSystem`) - ✅ (Concluído)
- [x] **InteractionScanner.cs:** Converter `Update`, `ScanForInteractables` para `virtual`.

---

## ⏱️ Previsão de Tempo Estimado (Framework + MVP)
Como Solo Dev (e tendo o Antigravity limpando a lógica suja pra você), com essa arquitetura clara de "Read-Only" e "Dogfooding", o foco muda de "criar sistemas gigantes" para "apenas usar os Legos".

*   **Fase 5 (Refatoração de "Protected Virtual" na Framework):** **1 a 2 horas**. É um trabalho braçal e cirúrgico apenas nas assinaturas dos métodos, sem precisar refazer as matemáticas por dentro.
*   **Fases 2.1 até 4 (Limpeza de RPG e Polimento do Shooter):** **2 a 3 dias de foco**. Principalmente para ajustar o *Game Feel* da câmera sobre os ombros (Hitscan) e tirar variáveis mágicas/fogo.
*   **Construção do MVP (Mini-jogo de Terror no projeto novo):** **1 a 2 semanas**. Como a Framework cuidará de todo o peso da locomoção e dano, você gastará esse tempo apenas configurando level design, atmosfera (Luzes/Sons), e ligando as Interfaces Gráficas aos *Events* da Framework.

**Resumo Final:** Você está a aproximadamente **10 a 14 dias de foco** de ter a Framework no seu estágio 100% Ouro e um protótipo de jogo próprio já rodando em cima dela de forma modular!

---
*Mantenha as coisas simples e modulares. Em caso de dúvida, pergunte-se: "Isso realmente é necessário para matar um zumbi em um corredor escuro?"*
