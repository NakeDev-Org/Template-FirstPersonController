# 🩸 NakeDev Template: FPS & Survival Horror Framework

Bem-vindo ao **NakeDev Template**! 
Este é o motor central focado na construção de experiências **Survival Horror** e **Action FPS** com peso, realismo e impacto.

Construído sob a rigorosa filosofia de ser ágil, pragmático e modular **(Foco total no Solo Dev)**, o NakeDev fornece a base sólida de Locomoção e Câmera FPS para que você possa focar 100% na Arte, Narrativa e no *Game Feel* do seu projeto, sem enlouquecer com código espaguete!

> Este pacote contém **apenas o sistema de primeira pessoa** (locomoção, câmera, input, gravidade, sway). Interação, Inventário, Inspeção de Itens e ferramentas de Editor viraram um pacote à parte, genérico e reaproveitável em qualquer perspectiva (1ª ou 3ª pessoa): **[NakeDev Core](https://github.com/NakeDev-Org/Template-CoreSystem)**. Instale os dois juntos — veja abaixo.

---

## ✨ Features (Construídas com a Regra de Ouro: KISS & YAGNI)

* **🏃‍♂️ Player Locomotion Limpo:** Um PlayerManager que atua como cérebro, com movimento fluido, câmera nativa de First Person pura e sistema modular de Sway para armas e câmera.
* **🪜 Stair Assist + Gravidade Ajustável:** Empurrão vertical automático ao esbarrar de frente num degrau baixo (sem precisar andar na diagonal), e gravidade calibrável pra um "peso de queda" mais realista.
* **🎮 Detecção de Dispositivo:** `InputReader` classifica automaticamente teclado/Xbox/PlayStation/Nintendo, usado pelo `IconInteraction` do pacote Core pra trocar o prompt de botão certo.
* **📼 Retro Animator (Opcional):** Transforme qualquer modelo 3D num clássico de PS1 com nosso script que trava o framerate das animações em 12 ou 15 FPS *(vem no pacote [NakeDev Core](https://github.com/NakeDev-Org/Template-CoreSystem))*.

---

## 🚀 Como Usar

Existem duas formas de usar o NakeDev Template, dependendo do seu fluxo de trabalho:

### Opção A — Como Pacote UPM (recomendado)

O framework vive dentro de `Assets/NakeDev-Template`, que é um pacote UPM completo (`package.json` + Assembly Definitions). Isso permite instalá-lo em qualquer projeto Unity via Git URL, sem copiar/colar código manualmente.

1. Abra `Packages/manifest.json` do seu projeto.
2. Adicione as duas dependências (o Unity Package Manager **não** resolve dependências git de forma transitiva — o `com.nakatimat.core` precisa ser listado manualmente, mesmo já estando declarado dentro do `package.json` deste pacote):
   ```json
   "com.nakatimat.template": "https://github.com/NakeDev-Org/template-fps-survivalhorror.git?path=/Assets/NakeDev-Template#master",
   "com.nakatimat.core": "https://github.com/NakeDev-Org/Template-CoreSystem.git"
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

## 🛠 Dependências
- **[NakeDev Core](https://github.com/NakeDev-Org/Template-CoreSystem):** obrigatório. Interação, Inventário, Inspeção de Itens e ferramentas de Editor vivem lá — instale junto (veja "Como Usar" acima).
- **Input System:** Novo sistema de eventos focado em Action Maps.

> *"Menos código solto = Menos bugs de madrugada."*
> - O Lema do Indie Dev.
