using UnityEngine;
using UnityEngine.Events;
using nakatimat.Core;
using nakatimat.Core.Inspector;

namespace nakatimat.InteractionSystem
{
    public class InteractionScanner : MonoBehaviour
    {
        [InspectorLine("Scanner Settings", 255, 200, 50)]
        [Tooltip("Câmera de onde o SphereCast será disparado")]
        [SerializeField] private Camera _fpsCamera;

        [Tooltip("Distância máxima para conseguir interagir")]
        [SerializeField] private float _scanDistance = 2f;
        
        [Tooltip("Espessura do raio (Aim Assist)")]
        [SerializeField] private float _sphereCastRadius = 0.2f;

        [SerializeField]
        private LayerMask _interactableLayer;

        [SerializeField]
        private float _scanInterval = 0.1f; // Não roda todo frame para poupar CPU

        // Auto-wire via GetComponent (interfaces não são arrastáveis no Inspector).
        private IInteractionInput _inputReader;

        [InspectorLine("Outputs (Regra 4)", 255, 150, 50)]
        [Tooltip("Disparado quando encontra um alvo válido. Retorna o IInteractable alvo.")]
        public UnityEvent<IInteractable> OnTargetDetected;

        [Tooltip("Disparado quando perde o alvo atual de vista.")]
        public UnityEvent OnTargetLost;

        private float _nextScanTime;
        private IInteractable _closestInteractable;

        protected virtual void Awake()
        {
            if (_fpsCamera == null)
            {
                _fpsCamera = Camera.main;
            }
        }

        protected virtual void OnEnable()
        {
            if (_inputReader == null)
                _inputReader = GetComponent<IInteractionInput>();
            if (_inputReader != null)
            {
                _inputReader.OnInteractionPressed += TryInteract;
            }
        }

        protected virtual void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnInteractionPressed -= TryInteract;
            }
        }

        protected virtual void Update()
        {
            if (Time.time >= _nextScanTime)
            {
                _nextScanTime = Time.time + _scanInterval;
                PerformScan();
            }
        }

        protected virtual void PerformScan()
        {
            if (_fpsCamera == null) return;

            IInteractable newClosest = null;

            // Dispara um SphereCast do centro da tela para frente
            Ray ray = _fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            if (Physics.SphereCast(
                ray, 
                _sphereCastRadius, 
                out RaycastHit hit, 
                _scanDistance, 
                _interactableLayer, 
                QueryTriggerInteraction.Collide))
            {
                newClosest = hit.collider.GetComponentInParent<IInteractable>();
            }

            // Atualiza estado se mudou o alvo
            if (newClosest != _closestInteractable)
            {
                // Limpa o alvo antigo
                if (_closestInteractable != null)
                {
                    _closestInteractable.SetTargeted(false);
                    OnTargetLost?.Invoke();
                }

                _closestInteractable = newClosest;

                // Configura o novo alvo
                if (_closestInteractable != null)
                {
                    _closestInteractable.SetTargeted(true);
                    OnTargetDetected?.Invoke(_closestInteractable);
                }
            }
        }

        protected virtual void TryInteract()
        {
            if (_closestInteractable != null)
            {
                _closestInteractable.Interact(this.gameObject);
            }
        }



        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            
            Camera cam = _fpsCamera != null ? _fpsCamera : Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                
                // Desenha a linha central (Ray)
                Gizmos.DrawRay(ray.origin, ray.direction * _scanDistance);
                
                // Desenha a esfera no final do raio para visualizar a espessura
                Gizmos.DrawWireSphere(ray.origin + ray.direction * _scanDistance, _sphereCastRadius);
            }
        }
    }
}

