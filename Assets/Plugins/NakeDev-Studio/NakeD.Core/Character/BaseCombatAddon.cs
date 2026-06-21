using System;
using nakatimat.Core.Interfaces;
using nakatimat.DamageSystem;
using nakatimat.RangedFramework;
using nakatimat.CombatSystem.MeleeSystem;
using UnityEngine;

namespace nakatimat.Core.Character
{
    public abstract class BaseCombatAddon : MonoBehaviour, ICombatAddon
    {
        [Header("Weapon Slots")]
        [SerializeField] protected Transform _weaponHandSlot;
        [SerializeField] protected Transform _weaponBackSlot;
        [SerializeField] protected Transform _defaultMuzzlePoint;

        [Header("Starting Equipment")]
        [SerializeField] protected TPSMeleeWeaponStats _startingMeleeWeapon;
        [SerializeField] protected RangedWeaponStats _startingRangedWeapon;

        // ICombatAddon
        public bool IsMeleeStance { get; protected set; }
        public bool IsBlocking { get; protected set; }


        // Events
        public event Action<TPSMeleeWeaponStats> OnMeleeWeaponEquipped;
        public event Action<RangedWeaponStats> OnRangedWeaponEquipped;
        public event Action<int, int> OnAmmoChanged; // current, max
        public event Action OnWeaponFired;

        // Melee Data
        protected TPSMeleeWeaponStats _currentMeleeWeapon;
        protected GameObject _meleeWeaponInstance;

        // Ranged Data
        protected RangedWeaponStats _currentRangedWeapon;
        protected GameObject _rangedWeaponInstance;
        protected RangedWeaponInstance _rangedWeaponInstanceRef;
        protected int _currentClipAmmo;
        protected bool _isReloading;
        protected float _lastShootTime;

        // Action Triggers for AnimationManager to consume
        public Action OnMeleeAttackTriggered;
        public Action OnShootTriggered;

        protected virtual void Start()
        {
            if (_startingMeleeWeapon != null) EquipMeleeWeapon(_startingMeleeWeapon);
            if (_startingRangedWeapon != null) EquipRangedWeapon(_startingRangedWeapon);
        }

        #region Melee Logic
        public virtual void EquipMeleeWeapon(TPSMeleeWeaponStats weapon)
        {
            if (weapon == null) return;

            if (_meleeWeaponInstance != null) Destroy(_meleeWeaponInstance);

            _currentMeleeWeapon = weapon;
            _meleeWeaponInstance = Instantiate(weapon.prefab, _weaponBackSlot);
            ResetWeaponTransform(_meleeWeaponInstance);

            OnMeleeWeaponEquipped?.Invoke(weapon);
        }

        public virtual void ToggleMeleeStance()
        {
            IsMeleeStance = !IsMeleeStance;
            
            if (IsMeleeStance)
            {
                if (_meleeWeaponInstance != null)
                {
                    _meleeWeaponInstance.transform.SetParent(_weaponHandSlot);
                    ResetWeaponTransform(_meleeWeaponInstance);
                }
                // Hide ranged weapon
                if (_rangedWeaponInstance != null) _rangedWeaponInstance.SetActive(false);
            }
            else
            {
                if (_meleeWeaponInstance != null)
                {
                    _meleeWeaponInstance.transform.SetParent(_weaponBackSlot);
                    ResetWeaponTransform(_meleeWeaponInstance);
                }
                // Show ranged weapon
                if (_rangedWeaponInstance != null) _rangedWeaponInstance.SetActive(true);
            }
        }

        public virtual void StartBlock() => IsBlocking = true;
        public virtual void StopBlock() => IsBlocking = false;
        #endregion

        #region Ranged Logic
        public virtual void EquipRangedWeapon(RangedWeaponStats weapon)
        {
            if (weapon == null) return;

            if (_rangedWeaponInstance != null) Destroy(_rangedWeaponInstance);

            _currentRangedWeapon = weapon;
            _currentClipAmmo = weapon.ClipSize;

            if (weapon.WeaponPrefab != null)
            {
                _rangedWeaponInstance = Instantiate(weapon.WeaponPrefab, _weaponBackSlot);
                ResetWeaponTransform(_rangedWeaponInstance);
                _rangedWeaponInstanceRef = _rangedWeaponInstance.GetComponent<RangedWeaponInstance>();
            }

            OnRangedWeaponEquipped?.Invoke(weapon);
            OnAmmoChanged?.Invoke(_currentClipAmmo, _currentRangedWeapon.ClipSize);
        }

        public virtual void HandleAimStarted()
        {
            if (IsMeleeStance || _isReloading) return;

            if (_rangedWeaponInstance != null)
            {
                _rangedWeaponInstance.transform.SetParent(_weaponHandSlot, false);
                ResetWeaponTransform(_rangedWeaponInstance);
            }
        }

        public virtual void HandleAimCanceled()
        {
            if (_rangedWeaponInstance != null)
            {
                _rangedWeaponInstance.transform.SetParent(_weaponBackSlot, false);
                ResetWeaponTransform(_rangedWeaponInstance);
            }
        }

        public virtual void TryShoot()
        {
            if (_currentRangedWeapon == null || IsMeleeStance || _isReloading) return;

            if (Time.time < _lastShootTime + _currentRangedWeapon.TimeBetweenShots) return;

            if (_currentClipAmmo <= 0)
            {
                Reload();
                return;
            }

            _lastShootTime = Time.time;
            _currentClipAmmo--;
            OnAmmoChanged?.Invoke(_currentClipAmmo, _currentRangedWeapon.ClipSize);

            PerformShoot();
        }

        protected abstract Ray GetShootRay();

        protected virtual void PerformShoot()
        {
            OnShootTriggered?.Invoke();
            OnWeaponFired?.Invoke();

            Ray ray = GetShootRay();
            
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _currentRangedWeapon.HitMask))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.ApplyDamage(_currentRangedWeapon.BaseDamage, gameObject);
                }
            }
        }

        public virtual void Reload()
        {
            if (_isReloading || _currentRangedWeapon == null || _currentClipAmmo >= _currentRangedWeapon.ClipSize) return;

            _isReloading = true;
            Invoke(nameof(FinishReload), _currentRangedWeapon.ReloadTime);
        }

        protected virtual void FinishReload()
        {
            _isReloading = false;
            _currentClipAmmo = _currentRangedWeapon.ClipSize;
            OnAmmoChanged?.Invoke(_currentClipAmmo, _currentRangedWeapon.ClipSize);
        }
        #endregion

        protected void ResetWeaponTransform(GameObject weapon)
        {
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }
        
        public TPSMeleeWeaponStats GetCurrentMeleeWeapon() => _currentMeleeWeapon;
    }
}
