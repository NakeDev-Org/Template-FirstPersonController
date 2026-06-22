using nakatimat.TPS.Player.Modular.Data;
using UnityEngine;
using nakatimat.Core.Inspector;

namespace nakatimat.TPS.Player.Modular
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(InputReader))]
    public class PlayerLocomotion : MonoBehaviour
    {
        [Separator("Dependencies", 255, 150, 50)]
        [SerializeField]
        private InputReader _inputReader;

        [SerializeField]
        private PlayerLocomotionStats _stats;
        public PlayerLocomotionStats Stats => _stats;

        [SerializeField]
        private PlayerCapsuleStats _capsuleStats;

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

            // Apply default Capsule Stats to the CharacterController
            if (_capsuleStats != null && _characterController != null)
            {
                _characterController.radius = _capsuleStats.Radius;
                _characterController.center = new Vector3(0f, _capsuleStats.StandingCenter, 0f);
                _characterController.height = _capsuleStats.StandingHeight;
            }
        }

        private bool _isAiming;
        private bool _isBlocking;

        public void HandleLocomotion(
            bool isSprinting,
            bool isBlocking,
            bool isMeleeCombat,
            bool isAiming
        )
        {
            if (_stats == null) return;

            IsSprinting = isSprinting;
            _isAiming = isAiming;
            _isBlocking = isBlocking;
            CalculateMoveDirection();
            UpdateSpeedState(
                isSprinting,
                isBlocking,
                isMeleeCombat,
                isAiming
            );
            ApplyMovementAndRotation();
        }

        public void HandleGravity()
        {
            GroundCheck();

            if (_stats == null) return;

            if (IsGrounded && VerticalVelocity < 0f)
            {
                VerticalVelocity = -2f; // Stick to ground
            }
            else
            {
                VerticalVelocity +=
                    Physics.gravity.y
                    * _stats.GravityMultiplier
                    * Time.deltaTime;
            }

            if (VerticalVelocity < _stats.TerminalVelocity)
            {
                VerticalVelocity = _stats.TerminalVelocity;
            }

            // Apply vertical velocity
            _characterController.Move(
                Vector3.up * VerticalVelocity * Time.deltaTime
            );
        }


        [SerializeField]
        private Transform _groundCheckPoint;

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

            if (_capsuleStats == null)
            {
                IsGrounded = false;
                return;
            }

            Vector3 spherePos;
            if (_groundCheckPoint != null)
            {
                spherePos = _groundCheckPoint.position;
            }
            else
            {
                spherePos =
                    transform.position
                    + Vector3.up * _capsuleStats.GroundedOffset;
            }

            IsGrounded = Physics.CheckSphere(
                spherePos,
                _capsuleStats.GroundCheckRadius + 0.05f,
                _capsuleStats.GroundLayerMask,
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
            bool isBlocking,
            bool isMeleeCombat,
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
                    targetSpeed = _stats.MeleeWalkSpeed * 0.7f;
                }
                else if (isBlocking)
                {
                    targetSpeed = _stats.MeleeWalkSpeed * 0.5f;
                }
                else if (isSprinting)
                {
                    targetSpeed = isMeleeCombat
                        ? _stats.MeleeSprintSpeed
                        : _stats.SprintSpeed;
                }
                else
                {
                    targetSpeed = isMeleeCombat
                        ? _stats.MeleeWalkSpeed
                        : _stats.WalkSpeed;
                }
            }

            CurrentSpeed = Mathf.Lerp(
                CurrentSpeed,
                targetSpeed,
                (targetSpeed < 0.01f ? 20f : GetAccelerationDamping()) * Time.deltaTime
            );
        }

        private float GetAccelerationDamping()
        {
            if (_stats == null) return 10f;
            switch (_stats.WeightProfile)
            {
                case MovementWeightProfile.Standard: return 4f; // Peso na aceleração (Motor)
                case MovementWeightProfile.Responsive: return 20f; // Instantâneo
                default: return 10f;
            }
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

            // Handle Rotation (OTS - Sempre alinhado à Câmera se movendo ou mirando)
            if (MoveDirection.sqrMagnitude > 0.01f || _isAiming)
            {
                if (_mainCamera != null)
                {
                    Vector3 camForward = _mainCamera.forward;
                    camForward.y = 0f;

                    if (camForward != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(
                            camForward.normalized,
                            Vector3.up
                        );
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            targetRotation,
                            _stats.RotationSmoothing * Time.deltaTime
                        );
                    }
                }
            }
        }

        public void SnapToInputDirection()
        {
            CalculateMoveDirection(); // Atualiza a direção com base no analógico atual

            if (MoveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(
                    MoveDirection,
                    Vector3.up
                );
                transform.rotation = targetRotation;
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
