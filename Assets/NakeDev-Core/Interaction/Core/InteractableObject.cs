using System.Collections.Generic;
using UnityEngine;

namespace nakatimat.InteractionSystem
{
    /// <summary>
    /// A generic MonoBehaviour you can attach to any 3D model in the scene to make it interactable.
    /// Uses ScriptableObjects to determine WHAT happens, adhering to SOLID principles.
    /// </summary>
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [Header("UI Prefab")]
        [Tooltip("Prefab de Screen Space Overlay que contém o script IconInteraction")]
        [SerializeField] private GameObject _iconPrefab;
        private nakatimat.InteractionSystem.UI.IconInteraction _myIcon;

        [Header("Modular Actions (Scriptable Objects)")]
        [Tooltip("Lista de ações modulares que serão executadas sequencialmente ao interagir.")]
        [SerializeField] private List<InteractionActionSO> _actionsToExecute = new List<InteractionActionSO>();

        public Vector3 GetUIPosition()
        {
            return transform.position;
        }


        public void Interact(GameObject interactor)
        {
            if (_actionsToExecute == null) return;

            foreach (var action in _actionsToExecute)
            {
                if (action != null)
                {
                    action.Execute(interactor, this.gameObject);
                }
            }
        }

        #region AUTO-CONFIGURAÇÃO (QoL)

        // O Reset roda automaticamente na Unity quando você arrasta esse script para um objeto pela primeira vez.
        private void Reset()
        {
            // 1. Auto-Adiciona um Collider se o artista 3D esqueceu
            if (GetComponent<Collider>() == null)
            {
                gameObject.AddComponent<BoxCollider>().isTrigger = false;
                Debug.Log(
                    $"[Auto-Config] BoxCollider adicionado em {gameObject.name}"
                );
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

        private void Start()
        {
            // Tenta achar caso você já tenha arrastado manualmente para dentro do item
            _myIcon = GetComponentInChildren<nakatimat.InteractionSystem.UI.IconInteraction>();

            // Se não achou, mas tem um prefab, ele cria AUTOMATICAMENTE DENTRO do item
            if (_myIcon == null && _iconPrefab != null)
            {
                GameObject uiObj = Instantiate(_iconPrefab, this.transform);
                _myIcon = uiObj.GetComponent<nakatimat.InteractionSystem.UI.IconInteraction>();
            }

            if (_myIcon != null)
            {
                _myIcon.Setup();
            }
        }

        public void SetTargeted(bool isTargeted)
        {
            if (_myIcon != null)
            {
                _myIcon.SetTargeted(isTargeted);
            }
        }

        /// <summary>
        /// Chamado pelo InspectSystem ao entrar/sair do modo de inspeção deste item:
        /// desliga o collider (evita que o InteractionScanner mire nele enquanto está na mão)
        /// e esconde o ícone de interação (que ficaria colado na câmera).
        /// </summary>
        public void SetInspecting(bool isInspecting)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = !isInspecting;
            }

            if (_myIcon != null)
            {
                _myIcon.gameObject.SetActive(!isInspecting);
            }
        }
    }
}
