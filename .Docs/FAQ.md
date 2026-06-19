# FAQ: Dúvidas Frequentes da NakeDev Framework

Este documento responde às dúvidas mais comuns sobre como integrar a Framework aos projetos finais, respeitando o [Anti-Roadmap](FRAMEWORK_ANTI_ROADMAP.md) e o Guia de Extensibilidade.

---

### 1. Vocês verificaram o Sistema de Interação na refatoração da Arquitetura?
**Sim!** O principal script lá é o `InteractionScanner.cs` (que procura itens ou portas na frente do jogador usando um OverlapSphere). Na refatoração (Fase 5 do Roadmap), os métodos dele se tornarão `protected virtual`. 
* **Por quê?** Se no seu jogo você quiser que a interação seja feita por um *Raycast* da câmera (estilo FPS) em vez de uma esfera na frente do corpo, você apenas herda o script e muda a matemática, sem tocar na Framework original. Além disso, as interações em si são guiadas pela interface `IInteractable`, permitindo que o seu jogo tenha portas com trancas numéricas ou baús de puzzle complexos.

---

### 2. No meu mini-projeto, por onde eu vou mexer no Inventário? Qual script cuida disso?
De acordo com o Anti-Roadmap, a Framework **não possui um inventário de mochila**. Ela possui um `CharacterEquipmentManager`, que gerencia exclusivamente "O que está na mão do jogador neste momento".

**Como você fará no seu Jogo:**
1. Você criará um script do zero no seu projeto, chamado algo como `PH_InventorySystem.cs`.
2. Esse script será responsável por abrir a maleta estilo Resident Evil 4, guardar as frutinhas e a munição reserva.
3. Quando o jogador "Equipar" uma arma na maleta, o seu `PH_InventorySystem` chama a Framework e diz: *"Ei `CharacterEquipmentManager`, o jogador equipou essa Pistola. Coloca na mão dele!"* (`equipmentManager.EquipWeapon(pistolStats)`).
A Framework engole a arma e passa a funcionar, sem nem saber que uma maleta gigante existe.

---

### 3. Onde serão centralizados os Loadings de Cena, Saves do jogo e Gestão de Cutscene?
Eles serão centralizados no seu próprio "Orquestrador" do jogo (A Lataria Mestre). A Framework cuida do *Momento a Momento* (Gameplay Tático), não do macro do jogo.

**Como você fará no seu Jogo:**
1. Você vai criar um `PH_GameInstance.cs` ou `PH_SaveManager.cs` no seu projeto.
2. **Saves:** O seu `SaveManager` lê um arquivo `.json` no PC do jogador que diz que ele tem 50 de vida. Seu script vai até a Framework e manda a ordem: `frameworkHealth.SetHealth(50)`.
3. **Cutscenes:** O seu script vai desativar o input da Framework (`playerInputReader.Disable()`), tocar uma Timeline do Unity (a cutscene), e quando acabar, devolver o controle para a Framework (`playerInputReader.Enable()`).
4. **Loadings:** A sua cena de menu será 100% independente. Quando o Loading terminar, você simplesmente "spawna" o Prefab do Jogador (que tem os scripts da Framework acoplados) no meio do cenário.

---

### 4. Como eu aplico efeitos de status (Veneno, Gelo, Sangramento) se as Armas da Framework agora são simples (Hitscan e Dano Bruto)?
Usando os **Eventos** e **Herança**.
1. Se uma arma de fogo do jogo precisar de dano de Fogo, você herda o `CharacterRangedCombat` e no método `PerformShoot()` (que é `virtual`), você executa o tiro original da base e instancia uma faísca.
2. O inimigo atingido vai disparar o evento `OnTakeDamage`. Se o seu inimigo no projeto final for um zumbi fraco a fogo, você lê esse evento no seu `ZombieBurnLogic.cs` e tira a vida extra por 5 segundos.
3. A Framework dá o dano bruto. O seu jogo aplica os "Enfeites" por cima usando a comunicação de Eventos.
