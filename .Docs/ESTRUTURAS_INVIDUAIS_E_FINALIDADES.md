# Estruturas Individuais e Finalidades (A Lataria)

Este documento detalha de forma direta como você (ou o seu cliente) deve estruturar a arquitetura do jogo final para suprir as áreas que a Framework ignora propositalmente (O [Anti-Roadmap](FRAMEWORK_ANTI_ROADMAP.md)).

A regra é simples: Um Manager (Maestro) rege o fluxo, e os sub-scripts (Pequenos) fazem o trabalho sujo. Use um prefixo no seu projeto (exemplo: `MYGAME_`) para diferenciar seus scripts da Framework.

---

## 1. Interface Gráfica (UI)
**Manager Principal:** `MYGAME_UIManager`
* **Finalidade:** Ligar e desligar painéis (HUD, Menu de Pause, Tela de Morte). Ele recebe a ordem do GameManager e mostra na tela.
* **Scripts Auxiliares Necessários:** 
  * `MYGAME_HealthBar` (Fica no Slider; apenas escuta o dano e esvazia a barra).
  * `MYGAME_AmmoCounter` (Fica no Texto; escuta o tiro e muda o número).

**Visão na Hierarchy:**
```text
▼ UI_Canvas
  - MYGAME_UIManager (Script)
  ▼ Pnl_GameplayHUD
    ▼ HealthBar_UI
      - MYGAME_HealthBar (Script)
    ▼ Ammo_UI
      - MYGAME_AmmoCounter (Script)
  ▼ Pnl_PauseMenu
```

---

## 2. Inventário e Itens
**Manager Principal:** `MYGAME_InventoryManager`
* **Finalidade:** Guardar a lista de itens, fazer a matemática de peso/espaço, e mandar a Framework equipar armas.
* **Scripts Auxiliares Necessários:**
  * `MYGAME_ItemData` (ScriptableObject com Nome e Ícone do item).
  * `MYGAME_ItemSlot` (Fica no botão da UI; sabe quando o mouse clicou nele).
  * `MYGAME_Pickup` (Fica no item 3D caído no chão da fase para ser coletado).

**Visão na Hierarchy:**
```text
▼ GameManager_Obj
  - MYGAME_InventoryManager (Script)
▼ UI_Canvas
  ▼ Pnl_InventoryGrid
    - Slot_1 (MYGAME_ItemSlot)
    - Slot_2 (MYGAME_ItemSlot)
▼ Environment
  - ArmaNoChao (MYGAME_Pickup)
```

---

## 3. Saves e Checkpoints
**Manager Principal:** `MYGAME_SaveManager`
* **Finalidade:** Converter a vida e os itens do jogador em um arquivo `.json` ou `PlayerPrefs`, e depois recarregá-los e injetar os números de volta na Framework.
* **Scripts Auxiliares Necessários:**
  * `MYGAME_SaveData` (Não vai na Hierarchy; é só um script C# puro com variáveis para virarem Json).
  * `MYGAME_Checkpoint` (Trigger invisível no mapa que avisa o SaveManager para salvar automaticamente).

**Visão na Hierarchy:**
```text
▼ GameManager_Obj
  - MYGAME_SaveManager (Script)
▼ Level_Design
  ▼ Checkpoints
    - Trigger_SafeRoom (MYGAME_Checkpoint)
```

---

## 4. Áudio Global (Música e Menus)
**Manager Principal:** `MYGAME_AudioManager`
* **Finalidade:** Tocar a música de fundo (BGM), gerenciar o Fade In/Fade Out das trilhas sonoras e tocar os cliques de Menu. *(Atenção: Os passos do jogador e sons de tiro são locais e resolvidos pela Framework no próprio Prefab).*
* **Scripts Auxiliares Necessários:**
  * `MYGAME_JumpscareTrigger` (Toca um som agudo quando o jogador pisa em um local específico).

**Visão na Hierarchy:**
```text
▼ AudioManager_Obj
  - MYGAME_AudioManager (Script)
  - AudioSource_BGM
  - AudioSource_SFX_Menus
▼ Level_Design
  - SustoCorredor (MYGAME_JumpscareTrigger)
```

---

## 5. Narrativa, Quests e Interação do Mapa
**Manager Principal:** `MYGAME_QuestManager`
* **Finalidade:** Rastrear o avanço do jogo ("Tem a chave azul?", "Matou o boss?").
* **Scripts Auxiliares Necessários:**
  * `MYGAME_LockedDoor` (Antes de abrir a porta, ele pergunta ao QuestManager se o jogador tem a chave).
  * `MYGAME_CutsceneTrigger` (Desativa o Input da Framework, roda a Cutscene da Unity, e devolve o controle).

**Visão na Hierarchy:**
```text
▼ GameManager_Obj
  - MYGAME_QuestManager (Script)
▼ Level_Design
  - Porta_Boss (MYGAME_LockedDoor)
  - Area_CutsceneInicial (MYGAME_CutsceneTrigger)
```
