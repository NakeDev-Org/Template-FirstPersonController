using UnityEngine;
using System;
using nakatimat.Core.Inspector;
using nakatimat.Core.Animation;

namespace nakatimat.Player
{
    public class PlayerLocomotionAnimation : MonoBehaviour
    {
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private AnimatorBrain _animatorBrain;
        [SerializeField] private PlayerLocomotion _playerLocomotion;

        // [OUTPUT] Chamar Som de Passos (Inscrito via código)
        public event Action OnFootstepAction;

        private readonly int _horizontalHash = Animator.StringToHash("Horizontal");
        private readonly int _verticalHash = Animator.StringToHash("Vertical");

        /// <summary>
        /// Método público chamado pelo Animator (Animation Events).
        /// </summary>
        public void TriggerFootstep()
        {
            OnFootstepAction?.Invoke();
        }

        private void Awake()
        {
            if (_animatorBrain == null)
            {
                _animatorBrain = GetComponent<AnimatorBrain>();
            }
            if (_inputReader == null)
            {
                _inputReader = GetComponentInParent<InputReader>();
            }
            if (_playerLocomotion == null)
            {
                _playerLocomotion = GetComponentInParent<PlayerLocomotion>();
            }
        }

        private void Update()
        {
            if (_animatorBrain == null || _inputReader == null) return;

            Vector2 input = _inputReader.MoveInput;
            
            bool isSprinting = _playerLocomotion != null && _playerLocomotion.IsSprinting;
            float maxY = isSprinting ? 1f : 0.5f;

            float x = Mathf.Clamp(input.x, -0.5f, 0.5f);
            float y = Mathf.Clamp(input.y, -0.5f, maxY);

            _animatorBrain.SetFloat(_horizontalHash, x);
            _animatorBrain.SetFloat(_verticalHash, y);
        }
    }
}

