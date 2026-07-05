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

            // Apply horizontal movement
            _characterController.Move(_currentMoveVelocity * Time.deltaTime);

            // Em FPS, a rotação do personagem já é feita instantaneamente pela câmera (PlayerFPSAimAddon),
            // então não precisamos girar o modelo/cápsula suavemente via código aqui.
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

