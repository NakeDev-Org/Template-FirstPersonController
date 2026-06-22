using nakatimat.Core.Character;
using UnityEngine;
using nakatimat.Core.Inspector;

namespace nakatimat.TPS.Player.Modular
{
    public class PlayerAnimationManager : BaseAnimationManager
    {
        // Internal References
        protected PlayerLocomotion _locomotion;

        protected override void Awake()
        {
            base.Awake();
            
            _locomotion = GetComponentInParent<PlayerLocomotion>();
            if (_locomotion == null) _locomotion = FindFirstObjectByType<PlayerLocomotion>();
        }

        protected override void UpdateLocomotion()
        {
            if (_locomotion == null || _animator == null) return;

            bool isMoving = _locomotion.MoveDirection.sqrMagnitude > 0.01f;
            
            Vector2 input = _locomotion.GetComponent<InputReader>().MoveInput;
            float h = 0f;
            float v = 0f;

            if (isMoving)
            {
                // Limita a 0.5 para Andar (Walk) e 1.0 para Correr (Sprint)
                float speedLimit = _locomotion.IsSprinting ? 1.0f : 0.5f;
                
                h = input.x * speedLimit;
                v = input.y * speedLimit;
            }

            _animator.SetFloat(_horizontalHash, h, _locomotionDampTime, Time.deltaTime);
            _animator.SetFloat(_verticalHash, v, _locomotionDampTime, Time.deltaTime);

            // Evita o problema visual da Unity de notação científica (ex: -2.34E-05) no Inspector
            if (!isMoving)
            {
                if (Mathf.Abs(_animator.GetFloat(_horizontalHash)) < 0.001f)
                    _animator.SetFloat(_horizontalHash, 0f);
                if (Mathf.Abs(_animator.GetFloat(_verticalHash)) < 0.001f)
                    _animator.SetFloat(_verticalHash, 0f);
            }

            _animator.SetBool(_isMovingHash, isMoving);
            _animator.SetBool(_isSprintingHash, _locomotion.IsSprinting);
        }
    }
}
