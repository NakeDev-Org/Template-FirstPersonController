using nakatimat.DamageSystem;
using UnityEngine;

namespace nakatimat.CombatSystem.MeleeSystem
{
    [RequireComponent(typeof(CharacterCombatAnimator))]
    public class CharacterEquipmentManager : MonoBehaviour
    {
        [Header("Ref")]
        [SerializeField]
        private CharacterCombatAnimator _animationHandler;

        [SerializeField]
        private GameObject _weaponInstance;

        [SerializeField]
        private Transform _weaponHandSlot;

        [SerializeField]
        private Transform _weaponBackSlot;

        // --- OBSERVER PATTERN (Eventos para Inventário/UI) ---
        public event System.Action<TPSMeleeWeaponStats> OnWeaponEquipped;
        public event System.Action OnWeaponUnequipped;

        [Header("Weapon Stats (Runtime)")]
        [SerializeField]
        private TPSMeleeWeaponStats _startingWeapon;

        [SerializeField]
        private TPSMeleeWeaponStats _currentWeaponData;

        [Header("Defense Runtime")]
        private bool _isBlocking;
        private float _parryEndTime;

        protected virtual void Awake()
        {
            if (_animationHandler == null)
            {
                _animationHandler = GetComponent<CharacterCombatAnimator>();
            }
        }

        protected virtual void Start()
        {
            if (_startingWeapon != null)
            {
                WeaponEquip(_startingWeapon);
            }
        }

        public virtual void WeaponEquip(TPSMeleeWeaponStats TPSMeleeWeaponStats)
        {
            if (TPSMeleeWeaponStats == null)
            {
                return;
            }

            if (_weaponInstance != null)
            {
                Destroy(_weaponInstance);
            }

            _currentWeaponData = TPSMeleeWeaponStats;

            _weaponInstance = Instantiate(
                TPSMeleeWeaponStats.prefab,
                _weaponBackSlot
            );
            ResetWeapon();
            _animationHandler.SetWeaponInHand(false);

            _animationHandler.EquipWeapon(TPSMeleeWeaponStats);
            OnWeaponEquipped?.Invoke(TPSMeleeWeaponStats);
        }

        public virtual void Attack()
        {
            if (_currentWeaponData == null)
            {
                return;
            }
            _animationHandler.Attack(_currentWeaponData.AttackData);
        }

        public virtual bool GetIsCombatMelee()
        {
            if (_animationHandler == null)
                return false;
            return _animationHandler.GetIsCombatMode();
        }

        public virtual bool IsAttacking()
        {
            if (_animationHandler == null)
                return false;
            return _animationHandler.IsAttacking();
        }

        public virtual float GetWeaponStaminaCost(float defaultCost = 15f)
        {
            return defaultCost;
        }

        public virtual void ToggleCombatMode()
        {
            if (_currentWeaponData == null)
            {
                return;
            }

            if (_animationHandler.GetIsCombatMode())
            {
                _animationHandler.ExitCombatMode();
            }
            else
            {
                _animationHandler.EnterCombatMode();
            }
        }

        public virtual void UnequipWeapon()
        {
            if (_weaponInstance != null)
            {
                Destroy(_weaponInstance);
            }
            _currentWeaponData = null;
            OnWeaponUnequipped?.Invoke();
        }

        #region Defense Logic
        public virtual void StartBlock()
        {
            if (!GetIsCombatMelee())
                return;

            _isBlocking = true;
            _animationHandler?.SetBlocking(true);
        }

        public virtual void StopBlock()
        {
            _isBlocking = false;
            _animationHandler?.SetBlocking(false);
        }

        public virtual bool IsBlocking()
        {
            return _isBlocking;
        }
        #endregion


        protected virtual void ResetWeapon()
        {
            _weaponInstance.transform.localPosition = Vector3.zero;
            _weaponInstance.transform.localRotation = Quaternion.identity;
        }

        #region Animation Event Callback
        public virtual void Equip()
        {
            _weaponInstance.transform.SetParent(_weaponHandSlot);
            ResetWeapon();
            _animationHandler.SetWeaponInHand(true);
        }

        protected virtual void Unequip()
        {
            _weaponInstance.transform.SetParent(_weaponBackSlot);
            ResetWeapon();
            _animationHandler.SetWeaponInHand(false);
        }
        #endregion
    }
}
