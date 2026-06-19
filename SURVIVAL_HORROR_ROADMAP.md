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

## ⏳ Phase 2.1: Purging RPG Elements from Melee (Próximo Passo)
- [ ] Simplificar o sistema de `MeleeWeaponStats`. Remover variáveis complexas de RPG (Dano de Fogo, Luz, Sangramento, etc).
- [ ] Focar em um sistema de dano simples, físico e direto (Raw Damage + Impacto).

## ⏳ Phase 3: Ranged Combat (Próximo Passo)
- [ ] Remover ou deletar componentes específicos para Arco e Flecha (Física balística complexa de projéteis lentos) do `NakeD.RangedFramework`.
- [ ] Refinar os scripts de armas de fogo (`RangedWeapon.cs`) para operar em modo **Hitscan** puro (Raycast saindo do centro da tela).
- [ ] Validar sistemas de consumo de munição (Pente) e Tempo de Recarga (Reload).

## ⏳ Phase 4: Camera & Aiming (Over The Shoulder)
- [ ] Eliminar o `TargetingSystem.cs` atual (que usa `Physics.OverlapSphere` para travar a mira automaticamente em inimigos como em Dark Souls).
- [ ] Substituir por um `AimController` focado em Mira Sobre o Ombro (OTS - Over The Shoulder).
- [ ] Fazer com que segurar a mira ("LT" / "Right Click") dê um zoom suave no FOV da Cinemachine, ative a retícula central na tela e trave a rotação do personagem perfeitamente na câmera.

---
*Mantenha as coisas simples e modulares. Em caso de dúvida, pergunte-se: "Isso realmente é necessário para matar um zumbi em um corredor escuro?"*
