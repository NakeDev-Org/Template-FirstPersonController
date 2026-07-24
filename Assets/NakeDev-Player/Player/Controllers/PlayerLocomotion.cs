using UnityEngine;
using nakatimat.Core.Inspector;

namespace nakatimat.Player
{


    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(InputReader))]
    public class PlayerLocomotion : MonoBehaviour
    {
        [InspectorLine("Dependencies", 255, 150, 50)]
        [SerializeField]
        private InputReader _inputReader;

        [InspectorLine("Movement Settings", 150, 200, 100)]
        [Header("Movement Speeds")]
        public float WalkSpeed = 3f;
        public float SprintSpeed = 6f;

        [Header("Aiming Speeds")]
        public float AimSpeed = 2f;

        [Header("Gravity")]
        public float GravityMultiplier = 2f;
        public float TerminalVelocity = -53f;

        [InspectorLine("Ground Check", 150, 200, 100)]
        [SerializeField] private Transform _groundCheckPoint;
        public float GroundCheckRadius = 0.28f;
        public float GroundedOffset = -0.14f;
        public LayerMask GroundLayerMask = -1;

        [InspectorLine("Stair Assist", 200, 150, 100)]
        [Tooltip("Empurra o personagem pra cima ao esbarrar de frente num degrau baixo (evita ter que andar na diagonal pra subir escada).")]
        public bool StairAssistEnabled = true;
        [Tooltip("Altura máxima de degrau que o auxílio ajuda a subir. Deve ficar perto do Step Offset do CharacterController.")]
        public float StairAssistMaxStepHeight = 0.35f;
        [Tooltip("Distância à frente checada em busca de degrau.")]
        public float StairAssistCheckDistance = 0.35f;
        [Tooltip("Velocidade do empurrão vertical aplicado ao detectar um degrau.")]
        public float StairAssistUpSpeed = 5f;



        // Optional dependencies
        private Transform _mainCamera;

        // Components
        private CharacterController _characterController;

        // State
        public bool IsGrounded { get; private set; }
        public float CurrentSpeed { get; private set; }
        public float VerticalVelocity { get; private set; }
        public Vector3 MoveDirection { get; private set; }
        public bool IsSprinting { get; private set; }
        private bool _isAiming;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            // Auto-wire dependencies if not set
            if (_inputReader == null)
                _inputReader = GetComponent<InputReader>();

            if (Camera.main != null)
            {
                _mainCamera = Camera.main.transform;
            }
        }

        public void HandleLocomotion(
            bool isSprinting,
            bool isAiming
        )
        {
            // Regra Survival Horror: Só corre se estiver com input para frente
            bool canSprint = isSprinting && _inputReader != null && _inputReader.MoveInput.y > 0.1f;
            
            IsSprinting = canSprint;
            _isAiming = isAiming;
            CalculateMoveDirection();
            UpdateSpeedState(
                IsSprinting, // Passamos a intent validada (canSprint)
                isAiming
            );
            ApplyMovementAndRotation();
        }

        public void HandleGravity()
        {
            GroundCheck();

            if (IsGrounded && VerticalVelocity < 0f)
            {
                VerticalVelocity = -2f; // Stick to ground
            }
            else
            {
                VerticalVelocity += Physics.gravity.y * GravityMultiplier * Time.deltaTime;
            }

            if (VerticalVelocity < TerminalVelocity)
            {
                VerticalVelocity = TerminalVelocity;
            }

            // Apply vertical velocity
            _characterController.Move(Vector3.up * VerticalVelocity * Time.deltaTime);
        }

        private void GroundCheck()
        {
            if (VerticalVelocity > 0.1f)
            {
                IsGrounded = false;
                return;
            }

            if (_characterController.isGrounded)
            {
                IsGrounded = true;
                return;
            }

            Vector3 spherePos;
            if (_groundCheckPoint != null)
            {
                spherePos = _groundCheckPoint.position;
            }
            else
            {
                spherePos = transform.position + Vector3.up * GroundedOffset;
            }

            IsGrounded = Physics.CheckSphere(
                spherePos,
                GroundCheckRadius + 0.05f,
                GroundLayerMask,
                QueryTriggerInteraction.Ignore
            );
        }

        protected virtual void CalculateMoveDirection()
        {
            if (_mainCamera == null || _inputReader == null)
                return;

            Vector2 input = _inputReader.MoveInput;

            Vector3 camForward = _mainCamera.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = _mainCamera.right;
            camRight.y = 0f;
            camRight.Normalize();

            MoveDirection = (camForward * input.y) + (camRight * input.x);
            if (MoveDirection.sqrMagnitude > 1f)
            {
                MoveDirection.Normalize();
            }
        }

        protected virtual void UpdateSpeedState(
            bool isSprinting,
            bool isAiming
        )
        {
            float inputMagnitude = new Vector2(
                MoveDirection.x,
                MoveDirection.z
            ).magnitude;
            float targetSpeed = 0f;

            if (inputMagnitude > 0.01f)
            {
                if (isAiming)
                {
                    targetSpeed = AimSpeed;
                }
                else if (isSprinting)
                {
                    targetSpeed = SprintSpeed;
                }
                else
                {
                    targetSpeed = WalkSpeed;
                }
            }

            CurrentSpeed = Mathf.Lerp(
                CurrentSpeed,
                targetSpeed,
                20f * Time.deltaTime
            );
        }

        private Vector3 _currentMoveVelocity;

        protected virtual void ApplyMovementAndRotation()
        {
            // Calculate what the speed SHOULD be based on input
            Vector3 targetVelocityXZ = MoveDirection * CurrentSpeed;
            bool isStopping = targetVelocityXZ.sqrMagnitude < 0.01f;

            if (IsGrounded)
            {
                // A direção responde rápido para o boneco não pilotar feito um barco,
                // O peso de verdade já foi calculado lá no CurrentSpeed.
                _currentMoveVelocity = Vector3.Lerp(
                    _currentMoveVelocity,
                    targetVelocityXZ,
                    (isStopping ? 20f : 15f) * Time.deltaTime
                );
            }
            else
            {
                // No ar, mantemos a inércia do pulo
                _currentMoveVelocity = Vector3.Lerp(
                    _currentMoveVelocity,
                    targetVelocityXZ,
                    2f * Time.deltaTime
                );
            }

            if (IsGrounded)
            {
                ApplyStairAssist();
            }

            // Apply horizontal movement
            _characterController.Move(_currentMoveVelocity * Time.deltaTime);

            // Em FPS, a rotação do personagem já é feita instantaneamente pela câmera (PlayerFPSAimAddon),
            // então não precisamos girar o modelo/cápsula suavemente via código aqui.
        }



        // Corrige o "enroscar" clássico do CharacterController em escadas/degraus quando o jogador
        // colide de frente (perpendicular) com o degrau: o Step Offset nativo às vezes só resolve
        // com um ângulo (por isso o hábito de andar na diagonal). Aqui checamos se o obstáculo à
        // frente é baixo (tem espaço livre logo acima) e, se sim, damos um empurrão vertical manual.
        // Cache só pra desenhar o Gizmo com o estado real do último check (ver OnDrawGizmosSelected)
        private bool _debugStairAssistActive;
        private Vector3 _debugLowOrigin, _debugHighOrigin, _debugDirection;
        private float _debugCastDistance;
        private bool _debugLowBlocked, _debugHighBlocked;

        private void ApplyStairAssist()
        {
            _debugStairAssistActive = false;

            if (!StairAssistEnabled)
                return;

            Vector3 horizontalVelocity = new Vector3(_currentMoveVelocity.x, 0f, _currentMoveVelocity.z);
            if (horizontalVelocity.sqrMagnitude < 0.01f)
                return;

            Vector3 direction = horizontalVelocity.normalized;
            float castDistance = _characterController.radius + StairAssistCheckDistance;

            // Raio baixo: na altura do "pé"/base da cápsula, onde a quina do degrau costuma bater.
            Vector3 lowOrigin = transform.position + Vector3.up * (_characterController.radius + 0.05f);
            bool lowBlocked = Physics.Raycast(lowOrigin, direction, castDistance, GroundLayerMask, QueryTriggerInteraction.Ignore);

            // Raio alto: na altura máxima de degrau permitida. Se também bater, é uma parede de verdade.
            Vector3 highOrigin = transform.position + Vector3.up * StairAssistMaxStepHeight;
            bool highBlocked = lowBlocked && Physics.Raycast(highOrigin, direction, castDistance, GroundLayerMask, QueryTriggerInteraction.Ignore);

            _debugStairAssistActive = true;
            _debugLowOrigin = lowOrigin;
            _debugHighOrigin = highOrigin;
            _debugDirection = direction;
            _debugCastDistance = castDistance;
            _debugLowBlocked = lowBlocked;
            _debugHighBlocked = highBlocked;

            if (!lowBlocked || highBlocked)
                return;

            _characterController.Move(Vector3.up * StairAssistUpSpeed * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            CharacterController controller = _characterController != null ? _characterController : GetComponent<CharacterController>();
            if (controller == null)
                return;

            if (_debugStairAssistActive)
            {
                // Estado real do último check em Play Mode
                Gizmos.color = _debugLowBlocked ? Color.yellow : Color.green;
                Gizmos.DrawLine(_debugLowOrigin, _debugLowOrigin + _debugDirection * _debugCastDistance);
                Gizmos.DrawWireSphere(_debugLowOrigin, 0.03f);

                Gizmos.color = _debugHighBlocked ? Color.red : Color.cyan;
                Gizmos.DrawLine(_debugHighOrigin, _debugHighOrigin + _debugDirection * _debugCastDistance);
                Gizmos.DrawWireSphere(_debugHighOrigin, 0.03f);
            }
            else
            {
                // Fora do Play Mode: mostra a área de checagem apontando pra frente do personagem
                Vector3 direction = transform.forward;
                float castDistance = controller.radius + StairAssistCheckDistance;
                Vector3 lowOrigin = transform.position + Vector3.up * (controller.radius + 0.05f);
                Vector3 highOrigin = transform.position + Vector3.up * StairAssistMaxStepHeight;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(lowOrigin, lowOrigin + direction * castDistance);

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(highOrigin, highOrigin + direction * castDistance);
            }
        }

        public void ApplyRootMotion(Animator animator)
        {
            if (_characterController != null && animator != null)
            {
                // Aplica o deslocamento físico da animação no CharacterController
                _characterController.Move(animator.deltaPosition);
                // Aplica a rotação da animação no Player
                transform.rotation *= animator.deltaRotation;
            }
        }
    }
}

