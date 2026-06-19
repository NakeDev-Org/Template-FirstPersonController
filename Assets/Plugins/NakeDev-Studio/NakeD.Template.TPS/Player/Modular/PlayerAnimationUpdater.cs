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

        // IK Components
        private HandIK _handIK;
        private ProceduralFootIK _footIK;

        private void Awake()
        {
            if (_locomotion == null)
                _locomotion = GetComponent<PlayerLocomotion>();
            if (_animatorHandler == null)
                _animatorHandler = GetComponent<CharacterAnimationHandler>();
            if (_inputReader == null)
                _inputReader = GetComponent<InputReader>();

            _handIK = GetComponent<HandIK>();
            _footIK = GetComponent<ProceduralFootIK>();
        }

        public void UpdateAnimations(
            bool isSprinting,
            bool isCrouching
        )
        {
            if (_locomotion == null || _animatorHandler == null)
                return;

            _animatorHandler.UpdateGrounded(_locomotion.IsGrounded);
            _animatorHandler.UpdateLocomotion(isCrouching);

            // Alimentamos o X e Y da BlendTree 2D sempre, pois agora é CameraStrafe constante
            if (_inputReader != null)
            {
                float h = _inputReader.MoveInput.x;
                float v = _inputReader.MoveInput.y;

                // Normaliza o input para não passar de 1, e depois multiplica pela velocidade real do personagem
                Vector2 input = new Vector2(h, v);
                if (input.sqrMagnitude > 1f)
                    input.Normalize();

                _animatorHandler.UpdateStrafeParameters(
                    input.x * _locomotion.CurrentSpeed,
                    input.y * _locomotion.CurrentSpeed
                );
            }

            if (_handIK != null)
            {
                _handIK.IsSprinting = isSprinting;
            }

            if (_footIK != null)
            {
                _footIK.IsSprinting = isSprinting;
            }
        }
    }
}
