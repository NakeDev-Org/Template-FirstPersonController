# 🧩 NakeDev Core: Interaction, Inventory & Inspection Systems

Sistemas genéricos e reaproveitáveis entre projetos e perspectivas (1ª ou 3ª pessoa), extraídos do [NakeDev Template — FPS & Survival Horror](https://github.com/NakeDev-Org/template-fps-survivalhorror). Não depende de nenhum controller de câmera ou locomoção específico.

---

## ✨ O que tem aqui

* **⚡ Sistema de Interação Zero-Code:** Baseado integralmente em *ScriptableObjects*. Adicione `InteractableObject` no objeto 3D e arraste a Ação (ScriptableObject) para ele no Inspector.
* **🎨 Biblioteca de Ícones por Dispositivo:** `InteractionIconSetSO` — um asset reutilizável com ícone por tipo de device (teclado, Xbox, PlayStation, Nintendo), plug-and-play em qualquer `InteractableObject`.
* **🔍 Sistema de Inspeção de Itens:** Puxa o item pra frente da câmera e gira com mouse ou stick pra examiná-lo antes de decidir coletar ou devolver ao mundo.
* **🎒 Inventário Extremamente Enxuto:** Sem dados pesados — dicionário veloz que escuta o jogo e guarda itens por ID (String).
* **📼 Retro Animator (Opcional):** Trava o framerate das animações em 12–15 FPS (efeito PS1), preservando blend trees e eventos.
* **🛠 Ferramentas de Editor:** `InspectorLineAttribute` (separadores coloridos no Inspector) e um Editor customizado para `InteractableObject`.

## 🔌 Como integrar com o seu InputReader

Esse pacote **não conhece** nenhum InputReader concreto — ele só depende da interface `nakatimat.Core.IInteractionInput`. Pra usar `InteractionScanner`, `IconInteraction` ou `InspectSystem`, seu próprio InputReader (de qualquer projeto, 1ª ou 3ª pessoa) precisa implementar:

```csharp
public interface IInteractionInput
{
    event Action OnInteractionPressed;
    event Action OnCancelPressed;
    Vector2 RawLookInput { get; }
    bool IsGamepad { get; }
    InputDeviceType CurrentDeviceType { get; }
}
```

No [NakeDev Template FPS](https://github.com/NakeDev-Org/template-fps-survivalhorror), o `InputReader` já implementa essa interface.

## 🚀 Instalação

Abra `Packages/manifest.json` do seu projeto e adicione:
```json
"com.nakatimat.core": "https://github.com/NakeDev-Org/Template-CoreSystem.git"
```

## 🛠 Dependências Nativas
- **uGUI:** para os ícones de interação em Screen Space (`IconInteraction`).
