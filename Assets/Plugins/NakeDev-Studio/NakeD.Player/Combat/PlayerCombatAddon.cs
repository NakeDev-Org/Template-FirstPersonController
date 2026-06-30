using System;
using UnityEngine;
using nakatimat.Core.Inspector;
using nakatimat.RangedFramework;
using nakatimat.CombatSystem.MeleeSystem;
using nakatimat.DamageSystem;

namespace nakatimat.TPS.Player.Modular
{
    [RequireComponent(typeof(InputReader))]
    public class PlayerCombatAddon : MonoBehaviour
    {
        [Separator("Combat Setup", 255, 100, 50)]
        [SerializeField] private Camera _fpsCamera;
        [SerializeField] private Transform _muzzlePoint;

        [Separator("Current Equipment", 100, 255, 100)]
        public RangedWeaponData CurrentRangedWeapon;
        public MeleeWeaponData CurrentMeleeWeapon;

        public bool IsMeleeStance = false;
        public bool IsBlocking = false;

        private InputReader _inputReader;
        private PlayerFPSAimAddon _aimingAddon;
        
        private float _lastShootTime = 0f;

        private void Awake()
        {
            _inputReader = GetComponent<InputReader>();
            _aimingAddon = GetComponent<PlayerFPSAimAddon>();
            
            if (_fpsCamera == null)
            {
                _fpsCamera = GetComponentInChildren<Camera>();
            }
        }

        private void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAttackPressed += HandleAttackPressed;
            }
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAttackPressed -= HandleAttackPressed;
            }
        }

        private void HandleAttackPressed()
        {
            if (IsMeleeStance)
            {
                PerformMeleeAttack();
            }
            else
            {
                // Must aim to shoot in some games, or we can allow hipfire. Let's allow hipfire unless strictly aimed.
                PerformRangedAttack();
            }
        }

        private void PerformMeleeAttack()
        {
            if (CurrentMeleeWeapon == null) return;

            // In a real scenario, this would trigger an animation (via CrossFade in AnimationManager)
            // and the animation would call an AnimationEvent to do the damage overlap.
            // For now, we simulate an instant overlap sphere in front of the player.
            
            Vector3 attackOrigin = transform.position + transform.forward * 1f + Vector3.up * 1.5f;
            Collider[] hits = Physics.OverlapSphere(attackOrigin, 1f);
            
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;
                
                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.ApplyDamage(CurrentMeleeWeapon.BaseDamage, gameObject);
                }
            }
        }

        private void PerformRangedAttack()
        {
            if (CurrentRangedWeapon == null) return;

            if (Time.time < _lastShootTime + CurrentRangedWeapon.TimeBetweenShots) return;
            _lastShootTime = Time.time;

            Ray ray;
            if (_fpsCamera != null)
            {
                ray = _fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }
            else
            {
                ray = new Ray(_muzzlePoint != null ? _muzzlePoint.position : transform.position, transform.forward);
            }

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, CurrentRangedWeapon.HitMask))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.ApplyDamage(CurrentRangedWeapon.BaseDamage, gameObject);
                }
            }
        }
    }
}
