using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nakatimat.TPS.Player.Modular
{
    /// <summary>
    /// Reads inputs from the new Input System and exposes them as public properties and events.
    /// This removes the Singleton dependency and allows other components to just listen to this reader.
    /// </summary>
    public class InputReader
        : MonoBehaviour,
            global::nakatimat.TPS.GeneratedInput.Controls.IMainActions,
            global::nakatimat.TPS.GeneratedInput.Controls.ICombatActions
    {
        private global::nakatimat.TPS.GeneratedInput.Controls _inputActions;

        // --- Properties (Stateful Inputs) ---
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsGamepad { get; private set; }

        // --- Events (Action Inputs) ---
        public event Action OnJumpPressed;
        public event Action OnAttackPressed;
        public event Action OnHeavyAttackPressed;
        public event Action OnSprintStarted;
        public event Action OnSprintCanceled;
        public event Action OnCrouchToggled;
        public event Action OnInteractionPressed;
        public event Action OnWeaponToggled;
        public event Action OnBlockStarted;
        public event Action OnBlockCanceled;
        public event Action OnAimStarted;
        public event Action OnAimCanceled;

        protected virtual void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions =
                    new global::nakatimat.TPS.GeneratedInput.Controls();
                _inputActions.Main.SetCallbacks(this);
                _inputActions.Combat.SetCallbacks(this);
            }
            _inputActions.Enable();
        }

        protected virtual void OnDisable()
        {
            if (_inputActions != null)
            {
                _inputActions.Disable();
            }
        }

        public virtual void OnMovement(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        public virtual void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();

            // Verifica se o dispositivo usado para olhar foi um controle (Gamepad)
            if (context.control != null && context.control.device != null)
            {
                IsGamepad = context.control.device is Gamepad;
            }
        }

        public virtual void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnJumpPressed?.Invoke();
        }

        public virtual void OnLightAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnAttackPressed?.Invoke();
        }

        public virtual void OnHeavyAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnHeavyAttackPressed?.Invoke();
        }

        public virtual void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnSprintStarted?.Invoke();
            if (context.canceled)
                OnSprintCanceled?.Invoke();
        }

        public virtual void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnCrouchToggled?.Invoke();
        }

        public virtual void OnInteraction(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnInteractionPressed?.Invoke();
        }

        public virtual void OnWeapon(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnWeaponToggled?.Invoke();
        }

        public virtual void OnBlockTargetParry(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnBlockStarted?.Invoke();
            if (context.canceled)
                OnBlockCanceled?.Invoke();
        }

        public virtual void OnAim(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnAimStarted?.Invoke();
            if (context.canceled)
                OnAimCanceled?.Invoke();
        }
    }
}
