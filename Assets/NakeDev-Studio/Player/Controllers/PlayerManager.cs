using System.Collections.Generic;
using UnityEngine;
using nakatimat.Core.Inspector;

namespace nakatimat.Player
{
    public enum PlayerState
    {
        Locomotion
    }

    /// <summary>
    /// The "Brain" of the player.
    /// Manages high-level state, interprets inputs into state changes, and coordinates the modules.
    /// This removes the spaghetti boolean logic from the God Class.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(InputReader))]
    [RequireComponent(typeof(PlayerLocomotion))]
    [RequireComponent(typeof(FirstPersonCamera))]
    public class PlayerManager : MonoBehaviour
    {
        [InspectorLine("State", 50, 255, 100)]
        public PlayerState CurrentState = PlayerState.Locomotion;

        [InspectorLine("Modules", 150, 100, 255)]
        [SerializeField]
        private InputReader _inputReader;

        [SerializeField]
        private PlayerLocomotion _locomotion;
        
        private FirstPersonCamera _firstPersonCamera;

        [Space]
        [SerializeField] private List<Renderer> playerRenderers;

        // Internal State
        private bool _isSprinting;

        protected virtual void Awake()
        {
            if (_inputReader == null)
                _inputReader = GetComponent<InputReader>();
            if (_locomotion == null)
                _locomotion = GetComponent<PlayerLocomotion>();
            _firstPersonCamera = GetComponent<FirstPersonCamera>();
            
            SetShadowsOnly();
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

            // Run state logic
            if (CurrentState == PlayerState.Locomotion)
            {
                UpdateLocomotionState();
            }


        }

        protected virtual void UpdateLocomotionState()
        {
            bool isAiming = _firstPersonCamera != null && _firstPersonCamera.IsAiming;

            // Cancel sprint if aiming
            if (_isSprinting && isAiming)
            {
                _isSprinting = false;
            }

            _locomotion.HandleLocomotion(
                _isSprinting,
                isAiming
            );

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



        private void SetShadowsOnly() 
        {
            foreach (var renderer in playerRenderers) 
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }
    }
}

