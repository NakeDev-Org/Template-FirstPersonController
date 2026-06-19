using nakatimat.TPS.Player.Modular.Data;
using UnityEngine;

namespace nakatimat.TPS.Player.Modular
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(InputReader))]
    public class PlayerLocomotion : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private InputReader _inputReader;

        [SerializeField]
        private PlayerLocomotionStats _stats;
        public PlayerLocomotionStats Stats => _stats;

        [SerializeField]
        private PlayerCapsuleStats _capsuleStats;

        // Optional dependencies
        private Transform _mainCamera;
        private TargetingSystem _targetingSystem;

        // Components
        private CharacterController _characterController;

        // State
        public bool IsGrounded { get; private set; }
        public float CurrentSpeed { get; private set; }
        public float VerticalVelocity { get; private set; }
        public Vector3 MoveDirection { get; private set; }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            // Auto-wire dependencies if not set
            if (_inputReader == null)
                _inputReader = GetComponent<InputReader>();

            // Assuming TargetingSystem might be on the same object
            _targetingSystem = GetComponent<TargetingSystem>();

            if (Camera.main != null)
            {
                _mainCamera = Camera.main.transform;
            }

            // Apply default Capsule Stats to the CharacterController
            if (_capsuleStats != null && _characterController != null)
            {
                _characterController.radius = _capsuleStats.Radius;
                SetCapsuleCrouchState(false); // Sets height and center to Standing mode
            }
        }

        private bool _isAiming;
        private bool _isBlocking;

        public void HandleLocomotion(
            bool isSprinting,
            bool isCrouching,
            bool isBlocking,
            bool isMeleeCombat,
            bool isAiming
        )
        {
            if (_stats == null) return;

            _isAiming = isAiming;
            _isBlocking = isBlocking;
            CalculateMoveDirection();
            UpdateSpeedState(
                isSprinting,
                isCrouching,
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
            bool isCrouching,
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
                else if (isCrouching)
                {
                    targetSpeed = _stats.CrouchSpeed;
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
                _stats.SpeedChangeDamping * Time.deltaTime
            );
        }

        private Vector3 _currentMoveVelocity;

        protected virtual void ApplyMovementAndRotation()
        {
            // Calculate what the speed SHOULD be based on input
            Vector3 targetVelocityXZ = MoveDirection * CurrentSpeed;

            if (IsGrounded)
            {
                // No chão, o personagem responde rápido aos controles
                _currentMoveVelocity = Vector3.Lerp(
                    _currentMoveVelocity,
                    targetVelocityXZ,
                    _stats.SpeedChangeDamping * Time.deltaTime
                );
            }
            else
            {
                // No ar, aplicamos a inércia: o atrito do ar é muito menor, então mantemos a velocidade do pulo
                _currentMoveVelocity = Vector3.Lerp(
                    _currentMoveVelocity,
                    targetVelocityXZ,
                    (_stats.SpeedChangeDamping * 0.15f) * Time.deltaTime
                );
            }

            // Apply horizontal movement
            _characterController.Move(_currentMoveVelocity * Time.deltaTime);

            // Handle Rotation
            bool hasTarget =
                _targetingSystem != null
                && _targetingSystem.GetCurrentTarget() != null;

            if (hasTarget)
            {
                Vector3 targetPos = _targetingSystem
                    .GetCurrentTarget()
                    .position;
                Vector3 directionToTarget = (
                    targetPos - transform.position
                ).normalized;
                directionToTarget.y = 0f;

                if (directionToTarget != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(
                        directionToTarget,
                        Vector3.up
                    );
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        _stats.RotationSmoothing * 2f * Time.deltaTime
                    );
                }
            }
            else
            {
                // Se estiver se movendo OU mirando, o personagem sempre olha para a frente da câmera (Strafe).
                // Se estiver parado e sem mirar, a câmera fica livre (personagem não gira).
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

        public void SetCapsuleCrouchState(bool isCrouching)
        {
            if (isCrouching)
            {
                _characterController.center = new Vector3(
                    0f,
                    _capsuleStats.CrouchingCenter,
                    0f
                );
                _characterController.height = _capsuleStats.CrouchingHeight;
            }
            else
            {
                _characterController.center = new Vector3(
                    0f,
                    _capsuleStats.StandingCenter,
                    0f
                );
                _characterController.height = _capsuleStats.StandingHeight;
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

        public bool CanStandUp()
        {
            if (_capsuleStats == null)
                return true;

            Vector3 standBottom =
                transform.position + Vector3.up * _capsuleStats.Radius;
            Vector3 standTop =
                transform.position
                + Vector3.up
                    * (_capsuleStats.StandingHeight - _capsuleStats.Radius);

            Collider[] hits = Physics.OverlapCapsule(
                standBottom,
                standTop,
                _capsuleStats.Radius,
                _capsuleStats.ObstacleMask,
                QueryTriggerInteraction.Ignore
            );
            foreach (var hit in hits)
            {
                if (hit.gameObject != this.gameObject)
                {
                    return false; // Found a REAL obstacle
                }
            }
            return true; // No obstacles other than ourselves
        }
    }
}
