# 🛠️ Como Montar Seu Personagem MVP (Template Playground)

Este é o guia passo-a-passo definitivo para plugar a **NakeDev Framework** em qualquer modelo 3D e montar um Player 100% funcional, pronto para o combate Survival Horror. 

Guarde este documento para usar quando for montar a sua cena de teste!

---

## 1. O Ambiente e a Câmera
Antes do personagem, precisamos do palco.
1. Crie uma cena limpa (Ex: `Template_Playground`).
2. Coloque um Chão simples (`Plane` ou `Cube`) e certifique-se de que ele esteja na camada (Layer) que o seu `PlayerCapsuleStats` considera como chão (ex: `Ground` ou `Default`).
3. **Cinemachine Setup:**
   - Adicione o componente `CinemachineBrain` na sua Main Camera.
   - Crie uma **Virtual Camera** para Locomoção (`vcam_Exploration`). Ela deve usar `3rd Person Follow` e `Composer` focados no personagem.
   - Crie uma segunda **Virtual Camera** para Mira (`vcam_Aiming`). Ela deve ter o FOV menor (ex: 40) e ficar grudada no ombro direito (Over The Shoulder).
4. **UI Setup:**
   - Crie um `Canvas` na cena.
   - Adicione uma pequena imagem branca `+` no meio da tela (este será o seu **Crosshair**).
   - Deixe o objeto do Crosshair DESLIGADO por padrão.

---

## 2. A Montagem do Esqueleto (O Player Prefab)
1. Importe o seu Modelo 3D (ex: um boneco Mixamo, Y-Bot, etc). Arraste-o para a cena.
2. Certifique-se de que o Animator do seu modelo esteja configurado como **Humanoid**.
3. No objeto raiz do seu Player, adicione o `CharacterController`. Ajuste a cápsula para cobrir o boneco perfeitamente.
4. Adicione o componente `Player Input` (da nova Input System) ou garanta que seu `InputReader` leia as ações globais.
5. **Injeção de Scripts (A Mágica):**
   Adicione os seguintes scripts no objeto raiz do Jogador:
   *   `PlayerManager`
   *   `PlayerLocomotion`
   *   `PlayerAnimationUpdater`
   *   `InputReader`
   *   `TPSCameraAimController` (Arraste o seu "CameraTarget", um objeto vazio no pescoço do personagem, para ele girar a câmera).
   *   `AimCameraController` (Arraste a `vcam_Aiming` e o objeto da UI do seu Crosshair nos slots desse script).
   *   `CharacterEquipmentManager`
   *   `CharacterCombatAnimator`
   *   `CharacterHealthManager`
   *   `CharacterRangedCombat` (O motor das armas de fogo).

---

## 3. O Cérebro Motor (Animator Controller)
O código não se mexe sozinho; ele pilota a Animator.
1. Crie um `Animator Controller` e coloque no seu Player.
2. **Criando a Locomoção:**
   - Crie um *Blend Tree* no State Machine.
   - Mude o tipo do Blend Tree para **2D Freeform Cartesian**.
   - Adicione os parâmetros float: `Horizontal` e `Vertical`.
   - Coloque suas animações de andar, correr e recuar nos 4 eixos. A Framework vai alimentar o `Horizontal` e `Vertical` sozinha baseada no analógico.
3. **Camada de Tronco (Upper Body):**
   - Na aba Layers do Animator, crie uma nova camada chamada `UpperBody`.
   - Mude o Blending para `Override` e o Weight para `1`.
   - Adicione uma **Avatar Mask** que bloqueie as pernas (deixe apenas o tronco verde).
   - É nesta camada que as animações de Atirar e Defender vão tocar. Dessa forma, você atira correndo!

---

## 4. Animation Rigging (A Coluna Procedural)
Para que a arma suba e desça acompanhando a câmera (sem precisar animar milhares de ângulos).
1. Instale o pacote **Animation Rigging** via Package Manager.
2. Selecione seu Player, vá em `Animation Rigging > Rig Setup`. Isso criará um componente `Rig Builder` e um objeto `Rig 1`.
3. Dentro de `Rig 1`, crie um objeto vazio e adicione o componente **Multi-Aim Constraint**.
4. Configure esse Constraint para atuar sobre o osso da **Spine / Chest** do seu modelo.
5. No campo *Source Objects* do Constraint, adicione um novo alvo (Target). Esse Target deve ser um objeto vazio que flutua a uns 50 metros na frente da Câmera!
6. No objeto do Player, adicione o script **`AimRigTargetController`** e arraste o seu Constraint e o Alvo para os slots dele.
   * *O script cuidará de dobrar a coluna do personagem APENAS quando você estiver mirando!*

---
Quando for testar, basta seguir este passo a passo e o seu TPS Survival Horror vai estar pronto para rodar, atirar e interagir!
