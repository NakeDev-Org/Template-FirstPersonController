using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace nakatimat.InteractionSystem
{
    /// <summary>
    /// A generic MonoBehaviour you can attach to any 3D model in the scene to make it interactable.
    /// Uses ScriptableObjects to determine WHAT happens, adhering to SOLID principles.
    /// </summary>
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField]
        private string _interactionPrompt = "Interact";

        [Tooltip("Opcional: Um ícone específico para este item (ex: Mão, Olho, Engrenagem).")]
        [SerializeField]
        private Sprite _interactionIcon;

        public string InteractionPrompt => _interactionPrompt;
        public Sprite InteractionIcon => _interactionIcon;

        [Header("Modular Actions (Scriptable Objects)")]
        [Tooltip(
            "Lista de ScriptableObjects que vão executar suas lógicas isoladas (ex: Tocar Som, Abrir Porta)."
        )]
        [SerializeField]
        private List<InteractionActionSO> _actionsToExecute;

        [Header("Unity Events (Optional Local Logic)")]
        [Tooltip(
            "Use isso se quiser chamar uma função específica de um script que já está neste objeto."
        )]
        [SerializeField]
        private UnityEvent _onInteract;

        // Opcional: Se você quiser um ícone flutuando acima da cabeça dele, você pode especificar um pivot
        [Header("UI Transform")]
        [Tooltip(
            "Deixe vazio para usar a raiz do objeto, ou coloque um filho mais alto para o ícone flutuar acima."
        )]
        [SerializeField]
        private Transform _uiPivot;

        [Tooltip(
            "Se você não quiser criar um objeto 'Pivot', basta usar este offset para empurrar o ícone para cima. (Ex: Y = 1.0)"
        )]
        [SerializeField]
        private Vector3 _uiOffset = new Vector3(0, 1.0f, 0);

        public Vector3 GetUIPosition()
        {
            if (_uiPivot != null)
                return _uiPivot.position;
            return transform.position + _uiOffset;
        }

        public void Interact(GameObject interactor)
        {
            // 1. Executa as ações genéricas dos ScriptableObjects (O modular perfeito)
            if (_actionsToExecute != null)
            {
                foreach (var actionSO in _actionsToExecute)
                {
                    if (actionSO != null)
                    {
                        actionSO.Execute(interactor, this.gameObject);
                    }
                }
            }

            // 2. Aciona o evento local (Útil para Level Design direto no Inspector)
            _onInteract?.Invoke();
        }

        #region AUTO-CONFIGURAÇÃO (QoL)

        // O Reset roda automaticamente na Unity quando você arrasta esse script para um objeto pela primeira vez.
        private void Reset()
        {
            // 1. Auto-Adiciona um Collider se o artista 3D esqueceu
            if (GetComponent<Collider>() == null)
            {
                gameObject.AddComponent<BoxCollider>().isTrigger = false;
                Debug.Log($"[Auto-Config] BoxCollider adicionado em {gameObject.name}");
            }

            // 2. Tenta forçar a Layer
            int layerIndex = LayerMask.NameToLayer("Interactable");
            if (layerIndex != -1)
            {
                gameObject.layer = layerIndex;
            }
        }

        private void Awake()
        {
            // Segurança em tempo de jogo: Garante que a Layer tá certa mesmo se alguém mudou sem querer
            int layerIndex = LayerMask.NameToLayer("Interactable");
            if (layerIndex != -1 && gameObject.layer != layerIndex)
            {
                gameObject.layer = layerIndex;
            }
        }

        #endregion
    }
}
