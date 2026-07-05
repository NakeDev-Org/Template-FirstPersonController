# 🎮 Guia de Testes do Template (Módulo Detalhado)

Se você quer testar as mecânicas na Unity do absoluto zero, siga **cada detalhe** deste guia. Um pequeno detalhe esquecido (como um Collider ou uma Layer) pode fazer o sistema inteiro parecer quebrado. Vamos lá!

---

## 1. O Jogador e o Scanner de Interação
O Player não é apenas movimento, ele também é quem "olha" para os objetos para interagir.

**Passo a passo:**
1. Abra a sua cena de testes.
2. Selecione o GameObject principal do seu **Player**. No Inspector dele, garanta que ele tem:
   - `CharacterController`
   - `InputReader`
   - `PlayerLocomotion`
   - `PlayerManager`
3. Selecione a **Câmera** do Jogador (Main Camera) na hierarquia. Ela precisa ter:
   - `FirstPersonCamera` (Nosso script renomeado).
   - `PlayerSwayAddon` (Com o Dropdown marcado para "Camera").
4. **O Scanner de Interação:** O jogador precisa saber olhar para os itens. Selecione o Player (ou a Câmera) e adicione o script **`InteractionScanner`**.
   - No Inspector do Scanner, você verá a variável **Interactable Layer**. Isso é CRUCIAL. Marque essa layer como `Default` ou crie uma layer específica chamada `Interactable` e marque-a. O Scanner só vai enxergar objetos que estiverem nessa Layer!

---

## 2. Onde fica o InventoryManager?
O `InventoryManager` é um script silencioso que fica ouvindo o jogo inteiro esperando um item ser coletado. Ele não tem corpo, apenas processa dados.

**Passo a passo:**
1. Na sua cena, crie um GameObject vazio (`GameObject > Create Empty`).
2. Mude o nome dele para **`GameManager`** (ou `Managers`).
3. Adicione o script **`InventoryManager`** neste objeto vazio.
   > **Nota:** Ele precisa estar na cena para funcionar! Se você quiser, pode colocar ele no próprio GameObject do Player também, funciona perfeitamente, já que ele apenas escuta eventos globais.

---

## 3. O Objeto Interagível (A Cobaia)
Vamos criar um item no mundo que você pode pegar e destruir. Ele precisa de massa (Collider) e pertencer ao mundo (Layer).

**Passo a passo:**
1. Na sua cena, crie um **Cubo** (`GameObject > 3D Object > Cube`). Coloque ele na frente do Player.
2. Selecione o Cubo. No topo do Inspector, procure a opção **Layer** e coloque na mesma Layer que você marcou lá no `InteractionScanner` do Jogador (ex: `Default` ou `Interactable`).
3. Verifique se o Cubo possui um **`BoxCollider`**. Se não tiver, o Scanner (que é um raio visual da câmera) vai atravessar ele direto e não vai detectar nada!
4. Adicione o script **`InteractableObject`** no Cubo. 
   - A interface vai ficar limpa, com os botões `+` e `-`.

---

## 4. Criando e Configurando o Item Coletável
Agora vamos transformar esse cubo inútil em uma Chave Mestra!

**Passo a passo:**
1. Na aba *Project* (seus arquivos), clique com o botão direito: `Create > NakeDev > Interaction > Actions > Functions > Collect Item`.
2. O arquivo será criado. Renomeie o arquivo para `Acao_ColetarChave`.
3. Selecione o `Acao_ColetarChave`. No Inspector dele:
   - **itemID:** Digite `chave_mestra` (em letras minúsculas e sem espaços, é a identidade secreta dele).
   - **amount:** Coloque `1`.
4. Volte para o Cubo que está na sua cena.
5. No script `InteractableObject` do Cubo, clique no botão **`+`**.
6. Um campo vazio vai aparecer. Arraste o seu arquivo `Acao_ColetarChave` (da aba Project) para dentro deste campo!
   > **Importante:** O script de coletar já destrói o objeto 3D automaticamente da cena, então você não precisa adicionar o "Destroy Object Action".

---

## 5. A Hora da Verdade (O Teste!)
1. Aperte o **Play** na Unity.
2. Ande até o Cubo.
3. Olhe para ele (coloque ele bem no centro da tela, que é de onde o raio do Scanner sai).
4. Aperte a tecla de interagir (geralmente **'E'**).
5. O Cubo **sumiu**!
6. Sem sair do Play, clique no seu objeto `GameManager` (ou onde você colocou o `InventoryManager`) na janela Hierarchy.
7. Vá na janela **Console** da Unity (Ctrl+Shift+C). Você deve ver uma mensagem brilhando lá: `[Inventory] Adicionado 1x 'chave_mestra'. Total: 1`.

Pronto! Você acaba de validar Câmera, Movimento, Raycast de Interação e Dicionário de Inventário sem escrever uma única linha de código, apenas brincando de LEGO no Inspector!
