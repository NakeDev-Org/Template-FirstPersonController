# 🩸 NakeDev Template: FPS & Survival Horror Framework

Bem-vindo ao **NakeDev Template**! 
Este é o motor central focado na construção de experiências **Survival Horror** e **Action FPS** com peso, realismo e impacto.

Construído sob a rigorosa filosofia de ser ágil, pragmático e modular **(Foco total no Solo Dev)**, o NakeDev fornece a base sólida de Locomoção, Câmera FPS, Interação, Inventário e Inspeção de Itens para que você possa focar 100% na Arte, Narrativa e no *Game Feel* do seu projeto, sem enlouquecer com código espaguete!

---

## ✨ Features (Construídas com a Regra de Ouro: KISS & YAGNI)

* **🏃‍♂️ Player Locomotion Limpo:** Um PlayerManager que atua como cérebro, com movimento fluido, câmera nativa de First Person pura e sistema modular de Sway para armas e câmera.
* **⚡ Sistema de Interação Zero-Code:** Baseado integralmente em *ScriptableObjects*. Quer que um cubo seja coletado ou uma porta seja aberta? Adicione o script `InteractableObject` no objeto 3D e arraste a Ação (ScriptableObject) para ele no Inspector. Simples assim.
* **🔍 Sistema de Inspeção de Itens:** Puxe o item pra frente da câmera e gire com mouse ou stick pra examiná-lo antes de decidir coletar ou devolver ao mundo.
* **🎒 Inventário Extremamente Enxuto:** Sem dados pesados. O `InventoryManager` é um dicionário veloz que escuta o jogo e guarda seus itens por ID (String). Tem visualização de Debug integrada ao Inspector.
* **📼 Retro Animator (Opcional):** Transforme qualquer modelo 3D num clássico de PS1 com nosso script que trava o framerate das animações em 12 ou 15 FPS.

---

## 🚀 Como Usar

Existem duas formas de usar o NakeDev Template, dependendo do seu fluxo de trabalho:

### Opção A — Como Pacote UPM (recomendado)

O framework vive dentro de `Assets/NakeDev-Template`, que é um pacote UPM completo (`package.json` + Assembly Definitions). Isso permite instalá-lo em qualquer projeto Unity via Git URL, sem copiar/colar código manualmente.

1. Abra `Packages/manifest.json` do seu projeto.
2. Adicione a dependência:
   ```json
   "com.nakatimat.template": "https://github.com/SEU_USUARIO/SEU_REPO.git?path=/Assets/NakeDev-Template#master"
   ```
3. Volte pro Unity Editor e deixe o Package Manager resolver a instalação.
4. Configure manualmente as **Tags** (`Player`) e **Layers** (`Interactable`, `InspectItem`, `ArmsLayers`) no seu projeto — elas não vêm com o pacote, pois ficam em `ProjectSettings` (fora do escopo de um pacote UPM).

Como é referenciado por Git URL, atualizar o template no seu projeto principal é só rodar o **Update** dele pelo Package Manager depois de um novo commit/push nesse repositório.

### Opção B — Clonando o projeto completo

Se quiser abrir esse repositório como um projeto Unity standalone (pra estudar, testar ou modificar o template com liberdade total):

1. **Requisito Mínimo:** Unity 6.x (6000.x+).
2. **Clone ou Baixe** este repositório (branch `master`).
3. Adicione a pasta no seu **Unity Hub** e abra o projeto.
4. Todo o coração da framework estará dentro da pasta: `Assets/NakeDev-Template`.
5. Modifique, estude e altere qualquer código! A arquitetura é sua.

---

## 🛠 Dependências Nativas
O pacote já vem pré-configurado utilizando as ferramentas oficiais mais recentes da Unity:
- **Input System:** Novo sistema de eventos focado em Action Maps.
- **uGUI:** Para os ícones de interação em Screen Space.

> *"Menos código solto = Menos bugs de madrugada."*
> - O Lema do Indie Dev.
