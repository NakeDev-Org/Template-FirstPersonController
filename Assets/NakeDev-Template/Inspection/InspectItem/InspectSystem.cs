using System.Collections.Generic;
using UnityEngine;
using nakatimat.Core;
using nakatimat.InteractionSystem;
using nakatimat.InventorySystem;

namespace nakatimat.InspectSystem
{
    /// <summary>
    /// Gerencia o modo de inspeção de itens: puxa o objeto interagido para um ponto na frente
    /// da câmera, deixa o jogador girá-lo livremente e decide o destino final via input:
    /// Interaction (confirma e envia pro inventário) ou Cancel (devolve ao mundo).
    /// </summary>
    public class InspectSystem : MonoBehaviour
    {
        [Tooltip("Arraste o InputReader do Player (ou da Cena) aqui")]
        [SerializeField] private nakatimat.Player.InputReader _inputReader;

        [Tooltip("SO opcional: se referenciado, alterna o GameState para 'Inspecting' ao inspecionar um item.")]
        [SerializeField] private GameStateSO _gameState;

        [Tooltip("SO opcional: guarda quais itemIDs o jogador já inspecionou, pra não pedir inspeção de novo em itens já conhecidos (mesmo que consumíveis e zerados no inventário).")]
        [SerializeField] private InspectedItemsRegistrySO _inspectedItemsRegistry;

        [Tooltip("Ponto (filho da InspectCamera) onde o item inspecionado é posicionado.")]
        [SerializeField] private Transform _inspectPoint;

        [Tooltip("Câmera overlay dedicada que renderiza apenas a layer 'InspectItem'. Fica desligada fora do modo de inspeção.")]
        [SerializeField] private Camera _inspectCamera;

        [Tooltip("Layer usada pelo item enquanto é inspecionado, para que só a InspectCamera o renderize (sem clipping com o mundo).")]
        [SerializeField] private string _inspectLayerName = "InspectItem";

        [SerializeField] private float _rotationSpeed = 4f; // Ajustado para o delta do mouse (pixels), com cursor travado

        [Tooltip("Velocidade de rotação usada com o stick do controle. Precisa ser bem maior que a do mouse, pois o gamepad manda um valor -1..1 (não um delta em pixels).")]
        [SerializeField] private float _gamepadRotationSpeed = 180f;

        private GameObject _currentTarget;
        private string _currentItemID;
        private int _currentAmount;

        private Transform _originalParent;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        private Rigidbody _currentRigidbody;
        private bool _originalIsKinematic;
        private bool _originalUseGravity;

        private readonly List<Transform> _affectedTransforms = new List<Transform>();
        private readonly List<int> _originalLayers = new List<int>();
        private int _inspectLayer = -1;

        // O InteractionScanner e este script escutam o mesmo evento OnInteractionPressed.
        // Sem essa guarda, o "E" que ENTRA no modo de inspeção também seria lido como o "E"
        // que CONFIRMA a coleta, dentro do mesmo disparo de evento.
        private int _beginInspectFrame = -1;

        public bool IsInspecting { get; private set; }

        /// <summary>
        /// Verdadeiro se o jogador já coletou esse itemID pelo menos uma vez antes (não precisa
        /// inspecionar de novo). Se nenhum registro estiver configurado, sempre retorna falso.
        /// </summary>
        public bool HasBeenInspected(string itemID) =>
            _inspectedItemsRegistry != null && _inspectedItemsRegistry.HasBeenInspected(itemID);

        private void OnEnable()
        {
            if (_inputReader == null)
                _inputReader = GetComponentInParent<nakatimat.Player.InputReader>();

            if (_inputReader != null)
            {
                _inputReader.OnInteractionPressed += HandleConfirmPressed;
                _inputReader.OnCancelPressed += HandleCancelPressed;
            }

            _inspectLayer = LayerMask.NameToLayer(_inspectLayerName);
            if (_inspectLayer == -1)
                Debug.LogWarning($"[InspectSystem] Layer '{_inspectLayerName}' não existe. Crie-a em Project Settings > Tags and Layers.");

            if (_inspectCamera != null)
                _inspectCamera.enabled = false;
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnInteractionPressed -= HandleConfirmPressed;
                _inputReader.OnCancelPressed -= HandleCancelPressed;
            }
        }

        private void Update()
        {
            if (!IsInspecting || _currentTarget == null) return;

            // Cursor travado durante a inspeção (GameStateSO): o item gira direto pelo
            // movimento do mouse/stick, sem precisar clicar e arrastar.
            //
            // Usando RawLookInput: o InputReader mantém esse valor vivo mesmo fora do estado
            // "Playing" (diferente de LookInput, que é zerado), então dá pra girar o item
            // com o mesmo mouse/stick da câmera sem mover o corpo do jogador.
            Vector2 lookDelta = _inputReader.RawLookInput;
            float speed = _inputReader.IsGamepad ? _gamepadRotationSpeed : _rotationSpeed;

            // Time.unscaledDeltaTime: continua girando mesmo se o timeScale for alterado.
            float rotationX = lookDelta.y * speed * Time.unscaledDeltaTime;
            float rotationY = -lookDelta.x * speed * Time.unscaledDeltaTime;

            Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
            _currentTarget.transform.localRotation = rotation * _currentTarget.transform.localRotation;
        }

        /// <summary>
        /// Inicia a inspeção de um item. Chamado pelo InspectItemActionSO ao interagir com um InteractableObject.
        /// </summary>
        public void BeginInspect(GameObject target, string itemID, int amount)
        {
            if (target == null || _inspectPoint == null || IsInspecting) return;

            _currentTarget = target;
            _currentItemID = itemID;
            _currentAmount = amount;

            _originalParent = target.transform.parent;
            _originalPosition = target.transform.position;
            _originalRotation = target.transform.rotation;

            target.GetComponent<InteractableObject>()?.SetInspecting(true);

            _currentRigidbody = target.GetComponent<Rigidbody>();
            if (_currentRigidbody != null)
            {
                _originalIsKinematic = _currentRigidbody.isKinematic;
                _originalUseGravity = _currentRigidbody.useGravity;

                _currentRigidbody.linearVelocity = Vector3.zero;
                _currentRigidbody.angularVelocity = Vector3.zero;
                _currentRigidbody.isKinematic = true;
                _currentRigidbody.useGravity = false;
            }

            target.transform.SetParent(_inspectPoint, worldPositionStays: false);
            target.transform.localPosition = Vector3.zero;
            target.transform.localRotation = Quaternion.identity;

            if (_inspectLayer != -1)
                SetLayerRecursive(target.transform, _inspectLayer);

            if (_inspectCamera != null)
                _inspectCamera.enabled = true;

            IsInspecting = true;
            _beginInspectFrame = Time.frameCount;

            if (_gameState != null)
                _gameState.ChangeState(GameState.Inspecting);
        }

        /// <summary>
        /// Muda a layer do item (e de todos os filhos) para a InspectItem, guardando a layer
        /// original de cada Transform para poder reverter se o jogador cancelar a inspeção.
        /// </summary>
        private void SetLayerRecursive(Transform root, int layer)
        {
            _affectedTransforms.Add(root);
            _originalLayers.Add(root.gameObject.layer);
            root.gameObject.layer = layer;

            foreach (Transform child in root)
                SetLayerRecursive(child, layer);
        }

        private void RestoreLayers()
        {
            for (int i = 0; i < _affectedTransforms.Count; i++)
            {
                if (_affectedTransforms[i] != null)
                    _affectedTransforms[i].gameObject.layer = _originalLayers[i];
            }

            _affectedTransforms.Clear();
            _originalLayers.Clear();
        }

        private void HandleConfirmPressed()
        {
            if (!IsInspecting) return;
            if (Time.frameCount == _beginInspectFrame) return;

            InventoryEvents.TriggerItemCollected(_currentItemID, _currentAmount);
            _inspectedItemsRegistry?.MarkInspected(_currentItemID);
            Destroy(_currentTarget);

            EndInspect();
        }

        private void HandleCancelPressed()
        {
            if (!IsInspecting) return;

            RestoreLayers();

            _currentTarget.transform.SetParent(_originalParent, worldPositionStays: false);
            _currentTarget.transform.SetPositionAndRotation(_originalPosition, _originalRotation);
            _currentTarget.GetComponent<InteractableObject>()?.SetInspecting(false);

            if (_currentRigidbody != null)
            {
                _currentRigidbody.isKinematic = _originalIsKinematic;
                _currentRigidbody.useGravity = _originalUseGravity;
            }

            EndInspect();
        }

        private void EndInspect()
        {
            if (_inspectCamera != null)
                _inspectCamera.enabled = false;

            _affectedTransforms.Clear();
            _originalLayers.Clear();

            _currentTarget = null;
            _currentItemID = null;
            _currentAmount = 0;
            _currentRigidbody = null;
            IsInspecting = false;

            if (_gameState != null)
                _gameState.ChangeState(GameState.Playing);
        }
    }
}
