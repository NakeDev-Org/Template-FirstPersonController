using System;
using nakatimat.AttackSystem;
using nakatimat.Core.Interfaces;
using nakatimat.DamageSystem;
using UnityEngine;

namespace nakatimat.CombatSystem.MeleeSystem
{
    [RequireComponent(typeof(TPSMeleeVFXController))]
    public class CharacterCombatAnimator : MonoBehaviour
    {
        [Header("Ref")]
        [SerializeField]
        private Animator _animator;
        private IMovementBlocker _movementBlocker;

        // --- OBSERVER PATTERN (Eventos) ---
        public event Action OnWeaponSwing;

        [SerializeField]
        private TPSMeleeVFXController _attackEffect;

        private int IsMeleeHash = Animator.StringToHash("IsMelee");
        private int TriggerEquipHash = Animator.StringToHash("Equip");
        private int TriggerUnequipHash = Animator.StringToHash("Unequip");
        private int IsAttackingHash = Animator.StringToHash("IsAttacking");
        private int AttackSpeedMultiplierHash = Animator.StringToHash("AttackSpeedMultiplier");
        private int IsBlockingHash = Animator.StringToHash("IsBlocking");

        [Header("Runtime - Combat State")]
        private bool _isMelee;
        private bool _buffAttack;
        private bool _isWeaponInHand;

        private AttackData _attackData; // Dano Base Injetado
        private TPSMeleeWeaponStats _currentWeaponData;
        private HandIK _handIK;
        private ProceduralFootIK _footIK;

        protected virtual void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            _movementBlocker = GetComponent<IMovementBlocker>();

            if (_attackEffect == null)
            {
                _attackEffect = GetComponent<TPSMeleeVFXController>();
            }

            _handIK = GetComponent<HandIK>();
            _footIK = GetComponent<ProceduralFootIK>();
        }

        protected virtual void Update()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            
            // Verifica se a animação de ataque terminou
            if (_buffAttack && stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= 0.98f)
            {
                ResetAttack();
            }

            bool isAttacking = _buffAttack;

            // --- Lógica IK ---
            if (_handIK != null)
            {
                if (isAttacking)
                {
                    _handIK.DisableRightHandIK = true;
                    _handIK.DisableLeftHandIK = true;
                }
                else if (_isMelee && _currentWeaponData != null)
                {
                    _handIK.DisableRightHandIK = _currentWeaponData.disableRightHandIK;
                    _handIK.DisableLeftHandIK = _currentWeaponData.disableLeftHandIK;
                }
                else
                {
                    _handIK.DisableRightHandIK = false;
                    _handIK.DisableLeftHandIK = false;
                }
            }

            if (_footIK != null)
            {
                _footIK.IsAttacking = isAttacking;
            }
        }

        public virtual void EquipWeapon(TPSMeleeWeaponStats TPSMeleeWeaponStats)
        {
            if (TPSMeleeWeaponStats == null)
                return;

            _currentWeaponData = TPSMeleeWeaponStats;

            bool previewIsMelee = _isMelee;
            _isMelee = false;
            _animator.SetBool(IsMeleeHash, _isMelee);

            if (TPSMeleeWeaponStats.animatorOverride != null)
            {
                _animator.runtimeAnimatorController = TPSMeleeWeaponStats.animatorOverride;
            }
            else
            {
                Debug.LogWarning($"No animator override assigned on TPSMeleeWeaponStats: {TPSMeleeWeaponStats.name}");
            }

            if (TPSMeleeWeaponStats.AttackData != null)
            {
                _animator.SetFloat(AttackSpeedMultiplierHash, TPSMeleeWeaponStats.AttackData.attackSpeedMultiplier);
            }

            if (previewIsMelee == true)
            {
                EnterCombatMode();
            }
        }

        public virtual void Attack(AttackData attackData)
        {
            if (attackData == null)
                return;

            _attackData = attackData;

            if (_isMelee == false)
            {
                _isMelee = true;
                _animator.SetBool(IsMeleeHash, _isMelee);
                GetComponent<CharacterEquipmentManager>()?.Equip();
            }

            if (_isWeaponInHand == false)
            {
                GetComponent<CharacterEquipmentManager>()?.Equip();
            }

            PerformAttack();
        }

        protected virtual void PerformAttack()
        {
            if (_currentWeaponData == null)
            {
                return;
            }

            _buffAttack = true;
            _animator.SetBool(IsAttackingHash, true);
            
            // Assume the default attack animation state name is "Attack1"
            _animator.CrossFadeInFixedTime("Attack1", 0.1f);
            
            _movementBlocker?.SetMovmentBlocked(true);
        }

        public virtual void ResetAttack()
        {
            _buffAttack = false;
            _animator.SetBool(IsAttackingHash, false);
            _movementBlocker?.SetMovmentBlocked(false);
        }

        public virtual bool GetIsCombatMode()
        {
            return _isMelee;
        }

        public virtual bool IsAttacking()
        {
            return _buffAttack;
        }

        public virtual void EnterCombatMode()
        {
            _isMelee = true;
            _animator.SetBool(IsMeleeHash, _isMelee);
            _animator.ResetTrigger(TriggerUnequipHash);
            _animator.SetTrigger(TriggerEquipHash);
        }

        public virtual void ExitCombatMode()
        {
            if (_isMelee == true)
            {
                _animator.ResetTrigger(TriggerEquipHash);
                _animator.SetTrigger(TriggerUnequipHash);
            }

            _isMelee = false;
            _animator.SetBool(IsMeleeHash, _isMelee);
        }

        public virtual void SetWeaponInHand(bool inHand)
        {
            _isWeaponInHand = inHand;
        }

        public virtual void SetBlocking(bool isBlocking)
        {
            if (_animator != null)
            {
                _animator.SetBool(IsBlockingHash, isBlocking);
            }
        }

        // Call this via Animation Event on the "Attack1" animation
        public virtual void Hit()
        {
            Vector3 center = transform.position + transform.forward * 1f + transform.up * 1f;
            Vector3 size = new Vector3(1f, 1f, 1f);
            _attackEffect?.TriggerAttack(center, size, _attackData);
            OnWeaponSwing?.Invoke();
        }
    }
}
