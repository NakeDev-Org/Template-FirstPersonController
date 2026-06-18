using nakatimat.Core.Interfaces;
using nakatimat.TPS.Player.Modular.Data;
using UnityEngine;

namespace nakatimat.TPS.Player.Modular
{
    public enum PlayerState
    {
        Locomotion,
        Airborne,
        ActionBlocked,
    }

    /// <summary>
    /// The "Brain" of the player.
    /// Manages high-level state, interprets inputs into state changes, and coordinates the modules.
    /// This removes the spaghetti boolean logic from the God Class.
    /// </summary>
    [RequireComponent(typeof(InputReader))]
    [RequireComponent(typeof(PlayerLocomotion))]
    [RequireComponent(typeof(PlayerAnimationUpdater))]
    [RequireComponent(typeof(TPSCameraAimController))]
    public class PlayerManager : MonoBehaviour, IMovementBlocker
    {
        [Header("State")]
        public PlayerState CurrentState = PlayerState.Locomotion;

        [Header("Modules")]
        [SerializeField]
        private TPSCameraAimController _cameraController;

        [SerializeField]
        private InputReader _inputReader;

        [SerializeField]
        private PlayerLocomotion _locomotion;
        private ICombatAddon _combatAddon;
        private IAimingAddon _aimingAddon;

        [SerializeField]
        private PlayerAnimationUpdater _animationUpdater;

        // Internal State
        private bool _isSprinting;
        private bool _isCrouching;
        private bool _isJumping;

        private void Awake()
        {
            if (_cameraController == null)
                _cameraController = GetComponent<TPSCameraAimController>();
            if (_inputReader == null)
                _inputReader = GetComponent<InputReader>();
            if (_locomotion == null)
                _locomotion = GetComponent<PlayerLocomotion>();
            _combatAddon = GetComponent<ICombatAddon>();
            _aimingAddon = GetComponent<IAimingAddon>();
            if (_animationUpdater == null)
                _animationUpdater = GetComponent<PlayerAnimationUpdater>();
        }

        private void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnJumpPressed += OnJumpPressed;
                _inputReader.OnSprintStarted += OnSprintStarted;
                _inputReader.OnSprintCanceled += OnSprintCanceled;
                _inputReader.OnCrouchToggled += OnCrouchToggled;
            }
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnJumpPressed -= OnJumpPressed;
                _inputReader.OnSprintStarted -= OnSprintStarted;
                _inputReader.OnSprintCanceled -= OnSprintCanceled;
                _inputReader.OnCrouchToggled -= OnCrouchToggled;
            }
        }

        private void Update()
        {
            if (_locomotion == null)
                return;

            // Handle Gravity Always
            _locomotion.HandleGravity();

            // Evaluate state transitions
            if (!_locomotion.IsGrounded && CurrentState != PlayerState.ActionBlocked)
            {
                CurrentState = PlayerState.Airborne;
            }
            else if (_locomotion.IsGrounded && CurrentState == PlayerState.Airborne)
            {
                CurrentState = PlayerState.Locomotion;
                _isJumping = false; // Reset jump state when landing
            }

            // Run state logic
            switch (CurrentState)
            {
                case PlayerState.Locomotion:
                    UpdateLocomotionState();
                    break;
                case PlayerState.Airborne:
                    UpdateAirborneState();
                    break;
                case PlayerState.ActionBlocked:
                    // Example: taking damage, dying, or in a hard-locked attack animation
                    // Don't process normal locomotion
                    break;
            }

            // Update Animations
            if (_animationUpdater != null)
            {
                _animationUpdater.UpdateAnimations(_isSprinting, _isCrouching, _isJumping);
            }
        }

        private void UpdateLocomotionState()
        {
            bool isMelee = _combatAddon != null && _combatAddon.IsMeleeStance;
            bool isBlocking = _combatAddon != null && _combatAddon.IsBlocking;
            bool isAiming = _aimingAddon != null && _aimingAddon.IsAiming;

            // Cancel sprint if aiming
            if (_isSprinting && isAiming)
            {
                _isSprinting = false;
            }

            // Stamina drain for sprinting
            if (
                _isSprinting
                && _combatAddon != null
                && _locomotion.Stats != null
                && _locomotion.Stats.RequireStaminaToSprint
            )
            {
                if (
                    !_combatAddon.HasEnoughStamina(
                        _locomotion.Stats.SprintStaminaCostPerSecond * Time.deltaTime
                    )
                )
                {
                    _isSprinting = false;
                    _locomotion.SetCapsuleCrouchState(_isCrouching);
                }
                else
                {
                    _combatAddon.TryConsumeStamina(
                        _locomotion.Stats.SprintStaminaCostPerSecond * Time.deltaTime
                    );
                }
            }

            // Pass isAiming parameter via the unused isBlocking slot, or we need to change PlayerLocomotion signature.
            // Wait, we need to change PlayerLocomotion to use isAiming.
            _locomotion.HandleLocomotion(_isSprinting, _isCrouching, isBlocking, isMelee, isAiming);

            if (!_locomotion.IsGrounded && _locomotion.VerticalVelocity < 0f)
            {
                // Falling
                CurrentState = PlayerState.Airborne;
            }
        }

        private void UpdateAirborneState()
        {
            bool isMelee = _combatAddon != null && _combatAddon.IsMeleeStance;
            bool isBlocking = _combatAddon != null && _combatAddon.IsBlocking;
            bool isAiming = _aimingAddon != null && _aimingAddon.IsAiming;

            // Maintain the current states in the air to prevent sudden deceleration
            _locomotion.HandleLocomotion(_isSprinting, _isCrouching, isBlocking, isMelee, isAiming);

            if (_locomotion.VerticalVelocity > 0.1f)
            {
                _isJumping = true;
            }
        }

        // --- Input Callbacks ---

        private void OnJumpPressed()
        {
            if (CurrentState != PlayerState.Locomotion)
                return;

            if (
                _combatAddon != null
                && _locomotion.Stats != null
                && _locomotion.Stats.RequireStaminaToJump
            )
            {
                if (!_combatAddon.HasEnoughStamina(_locomotion.Stats.JumpStaminaCost))
                    return;
                _combatAddon.TryConsumeStamina(_locomotion.Stats.JumpStaminaCost);
            }

            if (!_locomotion.IsGrounded)
                return;
            if (!_locomotion.CanStandUp())
                return;

            _isJumping = true;
            _isCrouching = false;
            _locomotion.SetCapsuleCrouchState(false);

            _locomotion.ProcessJump();
            CurrentState = PlayerState.Airborne;
        }

        private void OnSprintStarted()
        {
            if (CurrentState != PlayerState.Locomotion)
                return;

            if (
                _combatAddon != null
                && _locomotion.Stats != null
                && _locomotion.Stats.RequireStaminaToSprint
            )
            {
                if (
                    !_combatAddon.HasEnoughStamina(
                        _locomotion.Stats.SprintStaminaCostPerSecond * 0.1f
                    )
                )
                    return;
            }

            if (!_locomotion.CanStandUp())
                return;

            _isSprinting = true;
            _isCrouching = false;
            _locomotion.SetCapsuleCrouchState(_isCrouching);
        }

        private void OnSprintCanceled()
        {
            _isSprinting = false;
        }

        private void OnCrouchToggled()
        {
            if (CurrentState != PlayerState.Locomotion)
                return;
            if (!_locomotion.CanStandUp())
                return;

            _isCrouching = !_isCrouching;
            _locomotion.SetCapsuleCrouchState(_isCrouching);
        }

        /// <summary>
        /// Call this from external scripts (like taking damage or hard-locking attacks) to block movement.
        /// </summary>
        public void SetActionBlocked(bool isBlocked)
        {
            if (isBlocked)
            {
                CurrentState = PlayerState.ActionBlocked;
                _isSprinting = false;
            }
            else
            {
                CurrentState = _locomotion.IsGrounded
                    ? PlayerState.Locomotion
                    : PlayerState.Airborne;
            }
        }

        public void SetMovmentBlocked(bool value)
        {
            SetActionBlocked(value);

            // Se o movimento foi bloqueado (ex: por um ataque), podemos forçar
            // o personagem a rotacionar instantaneamente para a direção do analógico.
            if (value && _locomotion != null)
            {
                _locomotion.SnapToInputDirection();
            }
        }
    }
}
