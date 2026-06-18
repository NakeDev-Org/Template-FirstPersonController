using System;
using UnityEngine;

namespace nakatimat.TPS.Player
{
    public class CharacterAnimationHandler : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;

        private int CurrentSpeedHash = Animator.StringToHash("CurrentSpeed");
        private int IsCrouchingHash = Animator.StringToHash("IsCrouching");
        private int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private int IsJumpingHash = Animator.StringToHash("IsJumping");

        // 2D BlendTree Hashes
        private int HorizontalHash = Animator.StringToHash("Horizontal");
        private int VerticalHash = Animator.StringToHash("Vertical");

        // Combat/Aim Hashes
        private int IsAimingHash = Animator.StringToHash("IsAiming");

        // --- OBSERVER PATTERN (Eventos) ---
        public event Action<string> OnFootstep;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        public void UpdateLocomotion(float speed, bool isCrouching)
        {
            _animator.SetFloat(CurrentSpeedHash, speed);
            _animator.SetBool(IsCrouchingHash, isCrouching);
        }

        public void UpdateStrafeParameters(float horizontal, float vertical)
        {
            _animator.SetFloat(HorizontalHash, horizontal);
            _animator.SetFloat(VerticalHash, vertical);
        }

        public void UpdateGrounded(bool isGrounded)
        {
            _animator.SetBool(IsGroundedHash, isGrounded);
        }

        public void UpdateJumped(bool isJumping)
        {
            if (_animator == null)
                return;
            _animator.SetBool(IsJumpingHash, isJumping);
        }

        public void UpdateAiming(bool isAiming)
        {
            if (_animator == null)
                return;
            _animator.SetBool(IsAimingHash, isAiming);
        }

        // Chamado pelos Eventos de Animação (Animation Events) da Unity
        public void TriggerFootstep(string material = "Default")
        {
            OnFootstep?.Invoke(material);
        }
    }
}
