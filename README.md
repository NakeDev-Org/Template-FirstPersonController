# NakeDev TPS Framework

Bem-vindo à **NakeDev TPS Framework**! 
Este é o motor central focado na construção de experiências **Survival Horror** e **Action TPS** com peso, realismo e impacto. 

Construída sob a filosofia de ser ágil, pragmática e modular (para Solo Devs), a NakeDev Framework fornece toda a base complexa de Locomoção, Cinemachine e Combate para que você possa focar 100% na Arte, Narrativa e no *Game Feel* do seu projeto.

## 🚀 Instalação Rápida (UPM)

A maneira oficial e recomendada de instalar esta framework no seu jogo é através do **Unity Package Manager (UPM)**. Isso garante que o código permaneça limpo, intocável e receba atualizações facilmente.

1. Abra o seu novo projeto na Unity (Versão Recomendada: 2022.3 ou superior).
2. Acesse `Window > Package Manager`.
3. Clique no botão `+` (canto superior esquerdo) e escolha **"Add package from git URL..."**.
4. Copie e cole o link exato abaixo:

```text
https://github.com/NakeDev-Org/template-3d-tps.git?path=/Assets/Plugins/NakeDev-Studio
```

> **Nota para Desenvolvedores:** A framework injetará automaticamente as dependências necessárias, como *New Input System*, *Cinemachine* e *Animation Rigging*. Não modifique os scripts base diretamente! Se precisar estender uma funcionalidade, utilize Herança (`override`) ou assine os Eventos (`Actions/UnityEvents`) fornecidos pela framework em seus próprios scripts na pasta `Assets`.

### 📚 Pré-Requisitos de Estudo (O Poder da Framework)
Para extrair o máximo do design *Read-Only* desta framework sem nunca precisar alterar os scripts originais, é fundamental dominar os conceitos de Programação Orientada a Objetos abaixo. Eles ensinam como conectar as mecânicas específicas do seu jogo (Lataria) no nosso código base (Motor):

* **[Herança no C# (O Conceito de Pai e Filho)](https://learn.microsoft.com/pt-br/dotnet/csharp/fundamentals/object-oriented/inheritance)**
* **[A Palavra-chave 'base' (Executando as funções originais)](https://learn.microsoft.com/pt-br/dotnet/csharp/language-reference/keywords/base)**
* **[O Combo 'virtual' e 'override' (Sequestrando e Sobrescrevendo Matemática)](https://learn.microsoft.com/pt-br/dotnet/csharp/language-reference/keywords/override)**
