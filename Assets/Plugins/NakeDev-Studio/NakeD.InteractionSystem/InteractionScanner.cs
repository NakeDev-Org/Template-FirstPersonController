using System.Collections.Generic;
using nakatimat.InteractionSystem.UI;
using nakatimat.TPS.Player.Modular;
using UnityEngine;

namespace nakatimat.InteractionSystem
{
    public class InteractionScanner : MonoBehaviour
    {
        [Header("Scanner Settings")]
        [SerializeField]
        private float _scanRadius = 2f;

        [SerializeField]
        private LayerMask _interactableLayer;

        [SerializeField]
        private float _scanInterval = 0.1f; // Não roda todo frame para poupar CPU

        [Header("Dependencies")]
        [SerializeField]
        private InputReader _inputReader;

        [Tooltip("Arraste aquele IconInteraction que você criou no Canvas da Scene")]
        [SerializeField]
        private IconInteraction _interactionUI;

        private float _nextScanTime;
        private IInteractable _closestInteractable;
        private Collider[] _overlapResults = new Collider[10];

        private void OnEnable()
        {
            if (_inputReader == null)
                _inputReader = GetComponent<InputReader>();
            if (_inputReader != null)
            {
                _inputReader.OnInteractionPressed += TryInteract;
            }
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnInteractionPressed -= TryInteract;
            }
        }

        private void Update()
        {
            if (Time.time >= _nextScanTime)
            {
                _nextScanTime = Time.time + _scanInterval;
                PerformScan();
            }
        }

        private void PerformScan()
        {
            // Faz um OverlapSphere para achar qualquer coisa na Layer "Interactable"
            int hits = Physics.OverlapSphereNonAlloc(
                transform.position,
                _scanRadius,
                _overlapResults,
                _interactableLayer,
                QueryTriggerInteraction.Collide
            );

            IInteractable newClosest = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < hits; i++)
            {
                var interactable = _overlapResults[i].GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    float distance = Vector3.SqrMagnitude(
                        transform.position - interactable.GetUIPosition()
                    );
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        newClosest = interactable;
                    }
                }
            }

            // Atualiza estado se mudou o alvo
            if (newClosest != _closestInteractable)
            {
                _closestInteractable = newClosest;

                if (_closestInteractable != null)
                {
                    ShowInteractionUI(_closestInteractable);
                }
                else
                {
                    HideInteractionUI();
                }
            }
        }

        private void TryInteract()
        {
            if (_closestInteractable != null)
            {
                _closestInteractable.Interact(this.gameObject);
            }
        }

        private void ShowInteractionUI(IInteractable interactable)
        {
            if (_interactionUI != null)
            {
                // Injeta as informações do objeto (Ícone específico, texto de ação, etc)
                _interactionUI.Setup(interactable);

                // Liga o ícone e posiciona com o offset calculado
                _interactionUI.gameObject.SetActive(true);
                _interactionUI.transform.position = interactable.GetUIPosition();
            }
        }

        private void HideInteractionUI()
        {
            if (_interactionUI != null)
            {
                _interactionUI.gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _scanRadius);
        }
    }
}
