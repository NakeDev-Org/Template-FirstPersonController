# NakeDev Framework - Clean Template Architecture

Este documento define a arquitetura estrutural da template, baseada nos princípios KISS, YAGNI e alta performance.

## 1. Arquitetura do Player (Manager, Locomotion & Scriptable Objects)

Para garantir o equilíbrio perfeito entre **performance**, **modularidade** e **simplicidade (KISS)**, a estrutura do Player deve seguir regras rígidas:

### ❌ O que NÃO fazer (Over-engineering)
- **Não use ScriptableObjects para Lógica ou Estado:** Criar sistemas inteiros orientados a eventos e lógica rodando dentro de ScriptableObjects (SO Architecture complexa) adiciona overhead mental, dificulta o debug e gera lixo de memória se mutados em runtime.
- **Não crie "God Classes" (Classes Deus):** O `PlayerManager` não deve processar inputs ou mover o personagem diretamente.

### ✅ O Padrão Oficial
- **`PlayerManager` (MonoBehaviour):** É o **Hub Central**. Sua única função é fazer cache (em `Awake`) das referências para os subsistemas (Locomotion, Combat, Health, etc) e expor Estados globais legíveis (ex: `isSprinting`, `isAiming`). Zero lógica de update complexa.
- **`PlayerLocomotion` (MonoBehaviour):** É onde a mágica acontece. Lógica de `Update`/`FixedUpdate` de alta performance. Comunica-se com o CharacterController ou Rigidbody. Alocações zero.
- **Onde usar ScriptableObjects (ALERTA YAGNI):** **NÃO** usaremos SO para status de movimento do player. Como só existe um (1) player, variáveis como `WalkSpeed` e `JumpHeight` devem ser simples `[SerializeField]` dentro do próprio `PlayerLocomotion`. Criar um SO pra isso é over-engineering.
- **Onde usar Interfaces:** Para a locomoção básica e input do Player, **NÃO usaremos** interfaces. Manteremos um acoplamento direto via `PlayerManager` (estritamente como cache de referências, sem virar uma God Class) para garantir máxima performance.

---

## 2. Câmera e Weapon Sway (Animação Procedural)

Manteremos o *Sway* (balanço da arma e câmera) pois é fundamental para o *Game Feel* de um FPS Survival Horror. Nossa abordagem será **PURAMENTE FPS** (First-Person Shooter). Não haverá suporte ou código obsoleto para TPS, o que simplifica e otimiza ainda mais o motor da câmera.

### ✅ O Padrão Oficial (Otimização)
- **Foco Exclusivo em FPS:** A câmera será tratada apenas como Primeira Pessoa. Sem lógica de oclusão de paredes (Camera Collision) típica de TPS, reduzindo drasticamente cálculos de Raycast pesados.
- **Script Customizado Puro:** A menos que você exija o uso do Cinemachine, usaremos um script dedicado `PlayerCamera` (Standard API da Unity). Isso nos dá 100% de controle sobre a ordem de execução (rodando no `LateUpdate`) e evita o overhead de processamento de um sistema genérico de câmeras.
- **Zero Alocações por Frame:** O cálculo de Sway, Headbob e Recoil roda a todo momento. 
  - **Regra de Ouro:** Evite instanciar `new Vector3()` ou `new Quaternion()` dentro do `Update` ou `LateUpdate`. Reutilize variáveis locais cacheadas.
  - Prefira funções matemáticas diretas (`Mathf.Lerp`, `Quaternion.Euler`) em cima de variáveis já alocadas.
- **Onde usar ScriptableObjects (ALERTA YAGNI):** **NÃO** usaremos SO para configurações de Sway. Os limites de sway, velocidades e multiplicadores serão simples variáveis expostas no Inspector do `PlayerCamera`. Não adicione complexidade extra onde não precisa.
- **Onde usar Interfaces:** A câmera do FPS olhará para o mundo. Usaremos a interface **`IInteractable`** para o Raycast central. Se a câmera focar em um objeto que possua `IInteractable`, chamamos `Interact()` (útil para portas, itens), sem precisarmos saber qual script específico o objeto possui.
- **Desacoplamento do Input:** O script de Câmera e do Braço apenas **consome** os dados lidos pelo `PlayerManager`. Eles não acessam a `InputSystem` diretamente.
- **Sway Matemático Simples:** Calculado via interpolação de Quaternions, sem depender do Animator.

---

## 3. Combate Corpo a Corpo (Melee)

O sistema de combate melee deve ser brutal, direto e **KISS** (Keep It Simple, Stupid). Sem sistemas de combos, sem stunlocks complexos e absolutamente **ZERO elementos de RPG** (nada de dano crítico, multiplicadores de destreza ou escalonamento de status).

### ✅ O Padrão Oficial (Simplicidade Extrema)
- **Uma Animação, Múltiplas Armas:** Teremos apenas os braços em primeira pessoa e uma (1) animação padrão de ataque (um *swing* ou *stab* direto). A animação base é a mesma, trocando apenas o modelo 3D.
- **Onde usar ScriptableObjects:** O dano é bruto e imutável. Uma `MeleeWeaponSO` definirá qual modelo 3D carregar na mão e qual é o valor de dano (ex: Faca = 10, Cano = 25).
- **Onde usar Interfaces:** O ataque dispara um Raycast/Overlap. Tudo que puder receber dano (Zumbi, Barril, Vidro) deve implementar a interface **`IDamageable`**. O combate não precisa saber o que acertou, apenas tenta chamar `GetComponent<IDamageable>()?.TakeDamage(Weapon.Damage)`.
- **Sem Árvore de Habilidades ou Críticos:** A complexidade virá do *timing* e espaçamento, não de matemática invisível de RPG.

---

## 4. Combate a Distância (Armas de Fogo)

Teremos as três armas clássicas do survival horror: Pistola, Shotgun e Metralhadora. O sistema deve seguir o mesmo princípio brutal e direto do Melee. Sem balística complexa (bullet drop, velocidade de projétil) a menos que justificado, focando em **Hitscan** (Raycast).

### ✅ O Padrão Oficial (Simplicidade Extrema)
- **Dano Cravado:** Assim como no melee, o dano é fixo. Não há queda de dano por distância, não há chance de acerto crítico e não há RNG no valor. Se a pistola dá 10 de dano, ela dá 10 de dano, ponto final.
- **Onde usar ScriptableObjects:** **SIM**, usaremos. Criaremos uma `FirearmWeaponSO` (ou `RangedWeaponSO`). Como temos diferentes armas com diferentes comportamentos, a SO guardará as variáveis específicas de cada uma: `Damage`, `MagazineSize`, `FireRate`, `RecoilAmount` e o prefab do modelo 3D.
- **Onde usar Interfaces:** **SIM, a mesma do Melee.** Usaremos a interface **`IDamageable`**. O tiro é um simples `Physics.Raycast` a partir do centro da tela. Se bater num colisor, checamos: `hit.collider.GetComponent<IDamageable>()?.TakeDamage(Weapon.Damage)`. A mesma interface que atende a faca, atende a escopeta. Máximo reaproveitamento (DRY).

---

## 5. Sistema de Interação (Raycast & Eventos)

Essa é a ponte perfeita entre a Template e o Anti-Roadmap. O sistema de *Inventário em si* (grids, slots, peso) já está banido pelo nosso Anti-Roadmap, pois varia de jogo para jogo. **Porém, a mecânica física de olhar para algo e interagir PERTENCE à Template.**

### ✅ O Padrão Oficial (Simplicidade Extrema)
- **Foco apenas na Ação:** A Template cuida exclusivamente de lançar um Raycast (curto alcance) a partir da câmera para o centro da tela. Quando o jogador aperta o botão de ação (Ex: 'E' ou 'X'), o sistema tenta interagir com o objeto. O que acontece *depois* da interação não é problema do `PlayerManager`.
- **Onde usar ScriptableObjects:** Se a interação for um item de coleta, usaremos um SO genérico de Dados de Item (ex: `ItemDataSO`). A Template não processa o item, ela apenas o repassa para a frente via Eventos (`Action<ItemDataSO> OnItemCollected`). O seu jogo final escuta esse evento e decide se vai para a mochila ou se cai no chão porque está cheia.
- **Onde usar Interfaces:** **AQUI É O REINO DA INTERFACE.** Usaremos **`IInteractable`**.
  - O Raycast do jogador simplesmente procura essa interface no objeto que está olhando e chama `Interact(GameObject instigator)`.
  - Se for uma Porta (`class Door : MonoBehaviour, IInteractable`), ela se abre.
  - Se for um Curativo (`class PickupItem : MonoBehaviour, IInteractable`), ele emite o evento `OnItemCollected` passando seu SO e se destrói da cena (`Destroy(gameObject)`).
  - O Player nunca vai fazer um `GetComponent<Door>()` ou `GetComponent<PickupItem>()`. Totalmente desacoplado.

---

## 6. O que pode ser Compartilhado (Player vs Inimigos)

Para mantermos o código **DRY** (Não se repita), mas sem cairmos na armadilha de heranças gigantes (evite `Entity -> Actor -> Player/Enemy`), focaremos em **Composição** e no uso inteligente de Interfaces e SOs para compartilhar mecânicas.

### ✅ O Padrão Oficial (Reutilização Pragmática)
- **Onde usar Interfaces:** A interface **`IDamageable`** é universal! 
  - O `PlayerHealth.cs` implementa `IDamageable`. 
  - O `ZombieHealth.cs` implementa `IDamageable`. 
  - Quando o zumbi ataca, ele dispara um OverlapSphere. Se bater no Player, ele simplesmente chama `hit.GetComponent<IDamageable>()?.TakeDamage(Dano)`. É a exata mesma lógica que o Player usa para atacar o Zumbi. Zero duplicação.
- **Onde usar ScriptableObjects:** **SIM!** Os dados de armas (`MeleeWeaponSO` e `FirearmWeaponSO`) podem ser usados por Inimigos!
  - Se você tiver um Cultista com uma Escopeta, ele pode ler os mesmos dados (`Damage`, `FireRate`) do `FirearmWeaponSO` da escopeta.
  - As garras de um Zumbi podem ser definidas internamente como uma `MeleeWeaponSO` escondida na mão do inimigo, permitindo que você balanceie o dano do ataque dele no mesmo formato padronizado.
- **Evite Heranças Complexas (Composição > Herança):** Não crie um `CharacterManager` base gigante para derivar `PlayerManager` e `EnemyManager`. Mantenha-os separados. Eles apenas *compartilham as ferramentas* (Interfaces, SOs de armas, scripts isolados de física/dano), mas rodam suas lógicas de forma independente.

---

## 7. Sistema de Animação (KISS & CrossFade)

O *Animator Controller* da Unity pode se tornar um pesadelo (uma "teia de aranha" ilegível) se tentarmos gerenciar tudo via parâmetros (floats, bools, triggers) e dezenas de setinhas de transição visual. Vamos remover essa complexidade.

### ✅ O Padrão Oficial (Simplicidade Extrema)
- **Adeus Teia de Aranha (Sem Transições Visuais):** Não usaremos o visual graph do Animator para criar regras complexas de transição (ex: `If Speed > 0.1 AND isGrounded == true -> Walk`). 
- **Animação Orientada a Código (`CrossFade`):** Toda a lógica de qual animação deve tocar pertencerá ao código (ao script de Locomoção ou Combate). Nós usaremos a função `Animator.CrossFade("NomeDaAnimacao", tempoDeTransicao)` ou `Animator.Play()`. 
  - *Vantagem:* Você sabe exatamente qual parte do código tocou a animação. O debug é instantâneo e não há bugs de transições presas ou parâmetros esquecidos.
- **Uso de Hash para Performance:** Em vez de passar a string da animação (ex: `"Walk"`), usaremos o `Animator.StringToHash("Walk")` cacheado no `Awake` para máxima performance.
- **NADA de ScriptableObjects e Interfaces aqui:** A animação é estritamente vinculada ao visual e à lógica direta daquele modelo. Um script gerenciador simples (`PlayerAnimator` ou `EnemyAnimator`) é o suficiente para isolar as chamadas de `CrossFade`.
