# NakeDev Framework: Guia de Extensibilidade & "Dogfooding"

Este documento serve como a nossa bússola de Arquitetura. Ele alinha a nossa mentalidade para garantir que a **NakeDev Framework** seja robusta, modular e fácil de ser usada por você (no seu jogo) e por futuros parceiros/clientes.

## 🎯 O Objetivo: Dogfooding
**Dogfooding** significa "consumir o próprio produto". 
Nós vamos desenvolver um MVP (Mini-jogo de Survival Horror) usando a Framework importada via **UPM (Git URL)**.
Isso força a Framework a ser tratada como **Somente Leitura (Read-Only)**.
- Se sentirmos dificuldade em alterar algo no nosso jogo, significa que a Framework precisa expor melhor aquela funcionalidade.
- Evoluímos a Framework e o Jogo simultaneamente, mas com uma "parede" de código limpo entre eles.

---

## 🛠️ Como Expandir a Framework (Sem Modificar o Código Base)

Para que a Framework seja útil em um ambiente Read-Only, nós precisamos programar seus sistemas internos usando três pilares principais. Toda vez que formos mexer no núcleo (`NakeD.Core`, `NakeD.Combat`, etc), devemos nos perguntar: *"O usuário consegue alterar isso de fora?"*

### 1. Herança e Métodos `virtual` (O Padrão de Substituição)
Quando você cria um método normal na Framework, ele é fixo. Se você adicionar a palavra `virtual`, você dá "permissão" para que quem está fora crie um script que herde do original e **sobrescreva** (`override`) esse comportamento.

**Na Framework (Read-Only):**
```csharp
public class PlayerHealth : MonoBehaviour 
{
    // protected: Só esta classe e as que herdarem dela podem ver.
    protected int currentHealth;

    // virtual: Permite que outras classes modifiquem o que acontece aqui.
    public virtual void TakeDamage(int damage) 
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }
    
    protected virtual void Die() 
    {
        Debug.Log("Player morreu de forma padrão.");
    }
}
```

**No Jogo (Pasta Assets):**
```csharp
public class SurvivalPlayerHealth : PlayerHealth 
{
    // override: Sobrescreve a função original da Framework
    public override void TakeDamage(int damage) 
    {
        base.TakeDamage(damage); // Aplica o dano padrão
        CameraShake.Play();      // Adiciona o comportamento ESPECÍFICO do jogo!
    }
}
```

### 2. Arquitetura Orientada a Eventos (`Action` / `UnityEvent`)
A Framework não deve tentar adivinhar o que o jogo quer fazer. Ela deve apenas "avisar" que algo aconteceu.
Se o jogador atirar, a Framework dispara um evento `OnWeaponFired`. O jogo (na pasta Assets) assina esse evento e toca o som, diminui a munição da UI, etc.
- **Evita acoplamento:** A Framework não precisa saber que existe um Canvas na cena.

### 3. Composição (O Padrão Unity)
Sempre que possível, sistemas complexos devem ser quebrados em pequenos componentes (`MonoBehaviours`).
- Em vez de um script gigante `PlayerManager` que controla a Sanidade, o Cansaço e a Fome, nós deixamos o `PlayerManager` controlar só o básico.
- O usuário cria um script novo `SanitySystem` no jogo dele e anexa ao `PlayerPrefab`. O `SanitySystem` lê as variáveis do `PlayerManager` para saber quando o jogador está correndo no escuro e diminui a sanidade.

---

## 🚀 Próximos Passos
Toda vez que esbarramos em uma limitação enquanto fazemos nosso MVP de Survival Horror:
1. Anotamos a deficiência no ROADMAP.
2. Atualizamos o código da Framework (usando `virtual`, `protected` ou `Events`) e subimos para o Git.
3. Atualizamos a Package no Unity e usamos o novo recurso no Jogo!
