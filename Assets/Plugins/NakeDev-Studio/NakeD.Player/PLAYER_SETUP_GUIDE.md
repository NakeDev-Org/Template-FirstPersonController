# 🎮 Como Montar Seu Player (Guia Rápido)

Bem-vindo ao **NakeD.Player**! Montar o seu personagem principal nunca foi tão fácil. O sistema foi feito para ser prático: você adiciona os componentes como se fossem "peças de lego" dependendo do jogo que quer fazer.

Siga este passo a passo de 5 minutos para ter um Player completo rodando, mirando e atirando.

---

## Passo 1: Preparando o Modelo 3D
1. Arraste o seu modelo 3D para a Cena e descompacte o Prefab (`Unpack Prefab`).
2. Com o modelo selecionado, clique em **Add Component** e adicione:
   - **`Animator`**: (Coloque o seu Animator Controller nele).
   - **`Character Controller`**: (Ajuste o tamanho do cilindro verde para cobrir o corpo do seu personagem).

---

## Passo 2: Adicionando as Pernas e o Cérebro (Obrigatório)
Todo personagem que se move precisa das 3 "peças" essenciais. Selecione seu Player, clique em **Add Component** e adicione:

1. **`InputReader`** (Ele vai ler o seu teclado ou controle do Xbox/PlayStation automaticamente).
2. **`PlayerLocomotion`** (Faz o personagem andar e cair com gravidade).
3. **`PlayerManager`** (É o cérebro que organiza tudo).

> **O que eu preciso preencher no Inspector?**
> Apenas no `PlayerLocomotion`: Você precisará arrastar dois arquivos de **ScriptableObjects** (`PlayerLocomotionStats` e `PlayerCapsuleStats`) para definir a velocidade de caminhada, corrida e tamanho do personagem.
> 
> **💡 Como criar esses arquivos?** 
> Clique com o botão direito na aba **Project**, vá em `Create > NakeCore > TPS > Player` e escolha `Player Locomotion Stats` e `Player Capsule Stats`. Depois é só arrastar para o Inspector!

---

## Passo 3: Adicionando a Mira (Opcional - Apenas Jogos de Tiro)
Quer que o personagem puxe uma arma e aponte quando você apertar o botão direito?

1. Adicione o componente nativo **`Rig Builder`** (Da Unity).
2. Adicione o nosso script **`PlayerAimAddon`**.

> **O que eu preciso preencher no Inspector do `PlayerAimAddon`?**
> - **Camera Target**: Um objeto vazio na altura do pescoço (para a câmera seguir).
> - **Aim Camera**: A sua câmera virtual de mira (`vcam_Aiming`).
> - **Crosshair UI**: A imagem do alvo na tela.
> - **Spine Aim Constraint**: O osso da coluna que vai dobrar para mirar.
> - **Aim Target**: A bolinha invisível que fica flutuando na frente do player e serve como guia para as armas.

---

## Passo 4: Adicionando Armas e Combate (Opcional)
Para sacar espadas, pistolas e dar dano nos inimigos.

1. Adicione o script **`PlayerCombatAddon`**.
2. Adicione o script **`PlayerAnimationManager`** (Para tocar as animações de ataque).

> **O que eu preciso preencher no Inspector do `PlayerCombatAddon`?**
> - **Weapon Hand Slot**: O objeto da mão onde a arma vai ficar.
> - **Weapon Back Slot**: O objeto das costas onde a arma fica guardada.
> - **Starting Melee / Ranged Weapon**: Escolha qual pistola ou espada ele já começa usando.

---

## 🎯 É Só Isso! Dê Play!

Pronto! Se você fez um jogo medieval e não queria armas de fogo, era só pular o Passo 3. Se fez um jogo de terror (tipo Outlast) onde não tem combate, era só pular o Passo 3 e o Passo 4.

Você **monta o seu jogo apenas colocando ou tirando essas "peças de lego"**, sem precisar digitar uma linha de código!
