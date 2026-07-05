# 🩸 NakeDev Template: FPS & Survival Horror Framework

Bem-vindo ao **NakeDev Template**! 
Este é o motor central focado na construção de experiências **Survival Horror** e **Action FPS** com peso, realismo e impacto.

Construído sob a rigorosa filosofia de ser ágil, pragmático e modular **(Foco total no Solo Dev)**, o NakeDev fornece a base sólida de Locomoção, Interação e Inventário para que você possa focar 100% na Arte, Narrativa e no *Game Feel* do seu projeto, sem enlouquecer com código espaguete!

---

## ✨ Features (Construídas com a Regra de Ouro: KISS & YAGNI)

* **🏃‍♂️ Player Locomotion Limpo:** Um PlayerManager que atua como cérebro, com movimento fluido, câmera nativa de First Person pura e sistema modular de Sway para armas e câmera.
* **⚡ Sistema de Interação Zero-Code:** Baseado integralmente em *ScriptableObjects*. Quer que um cubo seja coletado ou uma porta seja aberta? Adicione o script `InteractableObject` no objeto 3D e arraste a Ação (ScriptableObject) para ele no Inspector. Simples assim.
* **🎒 Inventário Extremamente Enxuto:** Sem dados pesados. O `InventoryManager` é um dicionário veloz que escuta o jogo e guarda seus itens por ID (String). Tem visualização de Debug integrada ao Inspector.
* **📼 Retro Animator (Opcional):** Transforme qualquer modelo 3D num clássico de PS1 com nosso script que trava o framerate das animações em 12 ou 15 FPS.
* **🔧 Liberdade Total:** Nada de UPM (Unity Package Manager) bloqueando o seu código. Este template é seu! Você tem a posse do código e pode modificá-lo da forma que seu jogo precisar.

---

## 🚀 Como Usar (Instalação Limpa)

Esqueça o Unity Package Manager para o core do seu jogo. A NakeDev preza pela **liberdade de edição**.

1. **Requisito Mínimo:** Unity 6.x (6000.x+).
2. **Clone ou Baixe** este repositório (Recomendamos baixar diretamente da branch `master`).
3. Adicione a pasta extraída no seu **Unity Hub** e abra o projeto.
4. Todo o coração da framework estará dentro da pasta: `Assets/NakeDev-Template`.
5. Modifique, estude e altere qualquer código! A arquitetura é sua.

---

## 🛠 Dependências Nativas
O projeto já vem pré-configurado utilizando as ferramentas oficiais mais recentes da Unity:
- **Input System:** Novo sistema de eventos focado em Action Maps.
- **Cinemachine 3+:** Para manuseio avançado da câmera.

> *"Menos código solto = Menos bugs de madrugada."*
> - O Lema do Indie Dev.
