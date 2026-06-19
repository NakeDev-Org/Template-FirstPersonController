# O Anti-Roadmap: Os Limites da NakeDev Framework

Este documento é a nossa **Barreira de Fogo**. Como Solo Devs, a maior armadilha é o "Feature Creep" (tentar construir tudo). A NakeDev Framework é um motor de movimentação, combate e câmera (Core TPS). **Ela não é uma Engine completa.** 

Para garantir que a Framework seja lançada rápido (YAGNI/KISS) e permita que o usuário (ou você mesmo) aplique o próprio *Game Feel*, **A FRAMEWORK NUNCA DEVE FAZER AS SEGUINTES COISAS:**

---

## 🚫 1. Sistemas de UI (Canvas, HUD, Menus)
**A Framework NÃO PODE:**
- Instanciar ou referenciar `Canvas`, `Image`, `Text` ou `Slider`.
- Ter um `UIManager.cs`.
- Tocar na interface do usuário de forma alguma.

**Por quê?**
A UI dita 50% do *Game Feel*. Um inventário de Resident Evil (Grid) é totalmente diferente do The Last of Us (Slots). A cor, a fonte, a animação do sangue na tela... tudo isso é único para cada jogo.

**Como funciona então?**
A Framework apenas **emite Eventos**.
- *Framework:* `public event Action<int, int> OnAmmoChanged;`
- *Jogo (Assets):* Um script `HUDManager.cs` escuta esse evento e atualiza o texto na tela.

---

## 🚫 2. Sistemas de Inventário e Crafting
**A Framework NÃO PODE:**
- Ter listas de `List<Item>`, peso de mochila, ou lógicas de combinar erva verde com erva vermelha.
- Decidir como os itens são armazenados.

**Por quê?**
Sistemas de inventário são altamente complexos e variam drasticamente de jogo para jogo. Fazer um inventário genérico resulta em um código terrível de manter.

**Como funciona então?**
A Framework gerencia apenas o **Equipamento Ativo**. Ela sabe qual arma está na mão do jogador (`CurrentWeapon`) e quanta munição aquela arma tem no pente. Como a arma foi parar lá e como ela é guardada na mochila, é problema do Jogo.

---

## 🚫 3. Sistemas de Save & Load
**A Framework NÃO PODE:**
- Ter um `SaveManager.cs`.
- Escrever arquivos JSON, PlayerPrefs, ou comunicar com nuvem.

**Por quê?**
O que precisa ser salvo varia absurdamente. Um jogo salva posições, outro salva baús abertos, outro salva escolhas morais. 

**Como funciona então?**
O usuário cria o próprio sistema de Save no Jogo. Quando ele for salvar, ele lê as variáveis públicas do `PlayerManager` (ex: `player.CurrentHealth`, `player.transform.position`) e salva onde quiser.

---

## 🚫 4. Gerenciamento de Áudio (Music/SFX Managers)
**A Framework NÃO PODE:**
- Ter um `SoundManager.cs` que toca músicas de fundo.
- Hardcodar clipes de áudio dentro do código base (ex: `AudioSource.PlayClipAtPoint(clip)`).

**Por quê?**
O usuário pode querer usar a Unity nativa, FMOD, Wwise, ou sistemas customizados de áudio dinâmico. 

**Como funciona então?**
A Framework tem "Gatilhos" (Triggers/Eventos).
- *Framework:* "Acabei de dar um passo" (`OnFootstep`). "Acabei de atirar" (`OnShoot`).
- *Jogo:* Um script anexo ao jogador escuta isso e toca o som apropriado baseado no material do chão ou no tipo da arma.

---

## 🚫 5. Diálogos, Cutscenes e Quests
**A Framework NÃO PODE:**
- Ter árvores de diálogo.
- Controlar o fluxo narrativo do jogo.

**Por quê?**
Narrativa é o coração do Jogo, não da Framework. 

**Como funciona então?**
A Framework fornece **APIs de Controle**.
- *Jogo:* O sistema de cutscene do usuário chama `PlayerManager.Locomotion.DisableInput()`. A framework obedece. A cutscene rola. Depois o jogo chama `EnableInput()`. 

---

## 🚫 6. Progressão de RPG (XP, Skill Trees, Levels)
**A Framework NÃO PODE:**
- Ter variáveis como `Level`, `Experience`, ou atributos base (`Agility`, `Strength`).

**Por quê?**
Survival Horrors clássicos nem tem isso. Se o usuário quiser fazer um Action RPG depois, ele cria isso por fora. O combate da Framework deve ser focado em **Dano Físico Bruto e Impacto**.

**Como funciona então?**
O usuário cria um `PlayerRPGStats.cs` no jogo dele. Quando o jogador upar de nível, o script vai lá na Framework e modifica uma variável, ex: `PlayerManager.Combat.SetDamageMultiplier(1.5f)`.

---

## 🛑 O Teste de Ouro (KISS & YAGNI)
Antes de escrever qualquer código na pasta `NakeDev-Studio`, faça a si mesmo esta pergunta:
> *"Isso dita o visual, o som ou as regras de negócio de um jogo específico?"*
Se a resposta for SIM, **não programe**. Crie um Evento/Action e deixe que a pasta do projeto (`Assets/`) cuide disso. A Framework é o **Motor**, o jogo é a **Lataria**.
