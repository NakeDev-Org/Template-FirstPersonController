using UnityEngine;

namespace nakatimat.TPS.Player.Modular
{
    [RequireComponent(typeof(CharacterAnimationHandler))]
    public class PlayerAnimationUpdater : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private PlayerLocomotion _locomotion;

        [SerializeField]
        private CharacterAnimationHandler _animatorHandler;

        [SerializeField]
        private InputReader _inputReader;

        // IK Components (Removed)

        protected virtual void Awake()
        {
            if (_locomotion == null)
                _locomotion = GetComponent<PlayerLocomotion>();
            if (_animatorHandler == null)
                _animatorHandler = GetComponent<CharacterAnimationHandler>();
            if (_inputReader == null)
                _inputReader = GetComponent<InputReader>();

            // IK Components Removed
        }

        private Vector2 _currentBlendInput;

        public virtual void UpdateAnimations(
            bool isSprinting,
            bool isCrouching
        )
        {
            if (_locomotion == null || _animatorHandler == null)
                return;

            _animatorHandler.UpdateGrounded(_locomotion.IsGrounded);
            _animatorHandler.UpdateLocomotion(isCrouching);

            // Alimentamos o X e Y da BlendTree 2D sempre, limitando para as faixas corretas de animação
            if (_inputReader != null)
            {
                Vector2 input = _inputReader.MoveInput;
                
                // Normaliza para não passar de 1 em diagonais
                if (input.sqrMagnitude > 1f)
                    input.Normalize();

                // Regra do Survival Horror:
                // Se não está correndo, limita a magnitude em 0.5 (Walking)
                // Se está correndo, permite ir até 1.0 (Jogging)
                float maxBlend = isSprinting ? 1f : 0.5f;

                if (input.magnitude > maxBlend)
                {
                    input = input.normalized * maxBlend;
                }

                // Suavização manual para a animação transicionar fluidamente (evita snaps)
                _currentBlendInput = Vector2.Lerp(_currentBlendInput, input, Time.deltaTime * 10f);

                _animatorHandler.UpdateStrafeParameters(
                    _currentBlendInput.x,
                    _currentBlendInput.y
                );
            }


        }
    }
}
