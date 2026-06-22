using nakatimat.Core.Interfaces;
using nakatimat.TPS.Player.Modular.Data;
using UnityEngine;
using nakatimat.Core.Inspector;

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
    public class PlayerManager : MonoBehaviour, IMovementBlocker
    {
        [Separator("State", 50, 255, 100)]
        public PlayerState CurrentState = PlayerState.Locomotion;

        [Separator("Modules", 150, 100, 255)]
        [SerializeField]
        private InputReader _inputReader;

        [SerializeField]
        private PlayerLocomotion _locomotion;
        
        private ICombatAddon _combatAddon;
        private IAimingAddon _aimingAddon;

        // Internal State
        private bool _isSprinting;

        protected virtual void Awake()
        {
            if (_inputReader == null)
                _inputReader = GetComponent<InputReader>();
            if (_locomotion == null)
                _locomotion = GetComponent<PlayerLocomotion>();
            _combatAddon = GetComponent<ICombatAddon>();
            _aimingAddon = GetComponent<IAimingAddon>();
        }

        protected virtual void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnSprintStarted += OnSprintStarted;
                _inputReader.OnSprintCanceled += OnSprintCanceled;
            }
        }

        protected virtual void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnSprintStarted -= OnSprintStarted;
                _inputReader.OnSprintCanceled -= OnSprintCanceled;
            }
        }

        protected virtual void Update()
        {
            if (_locomotion == null)
                return;

            // Handle Gravity Always
            _locomotion.HandleGravity();

            // Evaluate state transitions
            if (
                !_locomotion.IsGrounded
                && CurrentState != PlayerState.ActionBlocked
            )
            {
                CurrentState = PlayerState.Airborne;
            }
            else if (
                _locomotion.IsGrounded
                && CurrentState == PlayerState.Airborne
            )
            {
                CurrentState = PlayerState.Locomotion;
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


        }

        protected virtual void UpdateLocomotionState()
        {
            bool isMelee = _combatAddon != null && _combatAddon.IsMeleeStance;
            bool isBlocking = _combatAddon != null && _combatAddon.IsBlocking;
            bool isAiming = _aimingAddon != null && _aimingAddon.IsAiming;

            // Cancel sprint if aiming
            if (_isSprinting && isAiming)
            {
                _isSprinting = false;
            }

            _locomotion.HandleLocomotion(
                _isSprinting,
                isBlocking,
                isMelee,
                isAiming
            );

            if (!_locomotion.IsGrounded && _locomotion.VerticalVelocity < 0f)
            {
                // Falling
                CurrentState = PlayerState.Airborne;
            }
        }

        protected virtual void UpdateAirborneState()
        {
            bool isMelee = _combatAddon != null && _combatAddon.IsMeleeStance;
            bool isBlocking = _combatAddon != null && _combatAddon.IsBlocking;
            bool isAiming = _aimingAddon != null && _aimingAddon.IsAiming;

            // Maintain the current states in the air to prevent sudden deceleration
            _locomotion.HandleLocomotion(
                _isSprinting,
                isBlocking,
                isMelee,
                isAiming
            );

            if (_locomotion.VerticalVelocity > 0.1f)
            {
                // Falling / Air behavior
            }
        }

        // --- Input Callbacks ---


        protected virtual void OnSprintStarted()
        {
            if (CurrentState != PlayerState.Locomotion)
                return;

            _isSprinting = true;
        }

        protected virtual void OnSprintCanceled()
        {
            _isSprinting = false;
        }

        /// <summary>
        /// Call this from external scripts (like taking damage or hard-locking attacks) to block movement.
        /// </summary>
        public virtual void SetActionBlocked(bool isBlocked)
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

        public virtual void SetMovmentBlocked(bool value)
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
