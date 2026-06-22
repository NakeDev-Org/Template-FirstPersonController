using nakatimat.Core.Interfaces;
using nakatimat.DamageSystem;
using nakatimat.CombatSystem.MeleeSystem;
using nakatimat.RangedFramework;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using nakatimat.Core.Inspector;

namespace nakatimat.Core.Character
{
    public abstract class BaseAnimationManager : MonoBehaviour
    {
        [Separator("Animator Components", 200, 50, 255)]
        [SerializeField] protected Animator _animator;
        [SerializeField] protected RigBuilder _rigBuilder;

        [Separator("Animation Damp Times", 150, 50, 200)]
        [SerializeField] protected float _locomotionDampTime = 0.1f;

        // State parameters hashes
        protected int _horizontalHash;
        protected int _verticalHash;
        protected int _isMovingHash;
        protected int _isSprintingHash;
        
        protected int _isCombatModeHash;
        protected int _isBlockingHash;
        protected int _weaponInHandHash;
        protected int _isAimingHash;

        // Triggers hashes
        protected int _attackTriggerHash;
        protected int _shootTriggerHash;
        protected int _hitTriggerHash;
        protected int _blockHitTriggerHash;

        // Internal References
        protected BaseCombatAddon _combatAddon;
        protected IAimingAddon _aimAddon;

        protected virtual void Awake()
        {
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (_rigBuilder == null) _rigBuilder = GetComponentInChildren<RigBuilder>();

            _combatAddon = GetComponentInParent<BaseCombatAddon>();
            _aimAddon = GetComponentInParent<IAimingAddon>();

            InitializeHashes();
        }

        protected virtual void InitializeHashes()
        {
            _horizontalHash = Animator.StringToHash("Horizontal");
            _verticalHash = Animator.StringToHash("Vertical");
            _isMovingHash = Animator.StringToHash("IsMoving");
            _isSprintingHash = Animator.StringToHash("IsSprinting");

            _isCombatModeHash = Animator.StringToHash("IsCombatMode");
            _isBlockingHash = Animator.StringToHash("IsBlocking");
            _weaponInHandHash = Animator.StringToHash("WeaponInHand");
            _isAimingHash = Animator.StringToHash("IsAiming");

            _attackTriggerHash = Animator.StringToHash("Attack");
            _shootTriggerHash = Animator.StringToHash("Shoot");
            _hitTriggerHash = Animator.StringToHash("Hit");
            _blockHitTriggerHash = Animator.StringToHash("BlockHit");
        }

        protected virtual void OnEnable()
        {
            if (_combatAddon != null)
            {
                _combatAddon.OnMeleeAttackTriggered += HandleMeleeAttack;
                _combatAddon.OnShootTriggered += HandleShoot;
                _combatAddon.OnMeleeWeaponEquipped += HandleMeleeEquipped;
                _combatAddon.OnRangedWeaponEquipped += HandleRangedEquipped;
            }
        }

        protected virtual void OnDisable()
        {
            if (_combatAddon != null)
            {
                _combatAddon.OnMeleeAttackTriggered -= HandleMeleeAttack;
                _combatAddon.OnShootTriggered -= HandleShoot;
                _combatAddon.OnMeleeWeaponEquipped -= HandleMeleeEquipped;
                _combatAddon.OnRangedWeaponEquipped -= HandleRangedEquipped;
            }
        }

        protected virtual void Update()
        {
            if (_animator == null) return;

            UpdateLocomotion();
            UpdateCombatAndAimState();
        }

        protected abstract void UpdateLocomotion();

        protected virtual void UpdateCombatAndAimState()
        {
            bool isMeleeStance = _combatAddon != null && _combatAddon.IsMeleeStance;
            bool isBlocking = _combatAddon != null && _combatAddon.IsBlocking;
            bool isAiming = _aimAddon != null && _aimAddon.IsAiming;

            _animator.SetBool(_isCombatModeHash, isMeleeStance);
            _animator.SetBool(_isBlockingHash, isBlocking);
            _animator.SetBool(_isAimingHash, isAiming);
            
            // Seta Weapon In Hand baseado no stance
            _animator.SetBool(_weaponInHandHash, isMeleeStance);
        }

        // --- Event Callbacks ---

        protected virtual void HandleMeleeAttack()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_attackTriggerHash);
        }

        protected virtual void HandleShoot()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(_shootTriggerHash);
            }
        }

        protected virtual void HandleMeleeEquipped(TPSMeleeWeaponStats weapon)
        {
            if (_animator == null || weapon == null) return;
            
            if (weapon.animatorOverride != null)
            {
                _animator.runtimeAnimatorController = weapon.animatorOverride;
            }
        }

        protected virtual void HandleRangedEquipped(RangedWeaponStats weapon)
        {
            if (_animator == null || weapon == null) return;
            
            if (weapon.animatorOverride != null)
            {
                _animator.runtimeAnimatorController = weapon.animatorOverride;
            }
        }

        // --- Animation Events ---
        
        public virtual void Equip() { }
        public virtual void Unequip() { }

        public virtual void EnableWeaponCollider() { }
        public virtual void DisableWeaponCollider() { }
    }
}
