using System;
using UnityEngine;
using UnityEngine.InputSystem;
using nakatimat.Core;
using nakatimat.Core.Inspector;

namespace nakatimat.Player
{
    /// <summary>
    /// Reads inputs from the new Input System and exposes them as public properties and events.
    /// This removes the Singleton dependency and allows other components to just listen to this reader.
    /// </summary>
    public class InputReader
        : MonoBehaviour,
            global::nakatimat.GeneratedInput.Controls.IMainActions
    {
        private global::nakatimat.GeneratedInput.Controls _inputActions;

        [InspectorLine("Configurações Globais", 255, 100, 50)]
        [Tooltip("Opcional: Se referenciado, o InputReader só lerá inputs de gameplay quando o estado for 'Playing'.")]
        [SerializeField] private GameStateSO _gameState;

        // --- Properties (Stateful Inputs) ---
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public Vector2 RawLookInput { get; private set; }

        public bool IsGamepad { get; private set; }

        // Verdadeiro enquanto o botão de rotação da inspeção (clique esquerdo do mouse) está pressionado.
        public bool IsInspectRotateHeld { get; private set; }

        // --- Events (Action Inputs) ---
        public event Action OnJumpPressed;
        public event Action OnSprintStarted;
        public event Action OnSprintCanceled;
        public event Action OnCrouchToggled;
        public event Action OnInteractionPressed;
        public event Action OnFlashlightToggled;
        public event Action OnAimStarted;
        public event Action OnAimCanceled;
        public event Action OnCancelPressed;

        protected virtual void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions =
                    new global::nakatimat.GeneratedInput.Controls();
                _inputActions.Main.SetCallbacks(this);
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
            if (_gameState != null && !_gameState.IsPlaying()) 
            {
                MoveInput = Vector2.zero;
                return;
            }

            MoveInput = context.ReadValue<Vector2>();
        }

        public virtual void OnLook(InputAction.CallbackContext context)
        {
            RawLookInput = context.ReadValue<Vector2>();

            if (_gameState != null && !_gameState.IsPlaying()) 
            {
                LookInput = Vector2.zero;
                return;
            }

            LookInput = RawLookInput;

            // Verifica se o dispositivo usado para olhar foi um controle (Gamepad)
            if (context.control != null && context.control.device != null)
            {
                IsGamepad = context.control.device is Gamepad;
            }
        }

        public virtual void OnJump(InputAction.CallbackContext context)
        {
            if (_gameState != null && !_gameState.IsPlaying()) return;

            if (context.performed)
                OnJumpPressed?.Invoke();
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
            // CanInteract() (em vez de IsPlaying()) permite reusar o mesmo botão para
            // iniciar a interação normal E confirmar a coleta enquanto o InspectSystem está ativo.
            if (_gameState != null && !_gameState.CanInteract()) return;

            if (context.performed)
                OnInteractionPressed?.Invoke();
        }

        public virtual void OnCancel(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnCancelPressed?.Invoke();
        }

        public virtual void OnInspectRotate(InputAction.CallbackContext context)
        {
            // O release sempre é processado (mesmo fora do estado "Playing"), pra nunca travar
            // IsInspectRotateHeld em true caso o GameState mude enquanto o botão está pressionado.
            if (context.canceled)
            {
                IsInspectRotateHeld = false;
                return;
            }

            if (_gameState != null && !_gameState.CanInteract()) return;

            if (context.performed)
                IsInspectRotateHeld = true;
        }

        public virtual void OnFlashlight(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnFlashlightToggled?.Invoke();
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

