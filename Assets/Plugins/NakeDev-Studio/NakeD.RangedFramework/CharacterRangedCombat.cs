using nakatimat.Core.Interfaces;
using nakatimat.TPS.Player.Modular;
using UnityEngine;
using UnityEngine.Events;

namespace nakatimat.RangedFramework
{
    [RequireComponent(typeof(CharacterAimController))]
    [RequireComponent(typeof(AimCameraController))]
    [RequireComponent(typeof(RangedAnimationUpdater))]
    public class CharacterRangedCombat : MonoBehaviour
    {
        [Header("Weapon Data")]
        [Tooltip(
            "Arraste o ScriptableObject com as informações da sua Arma de Fogo"
        )]
        [SerializeField]
        protected RangedWeaponStats _currentWeapon;

        [Header("Equipment Slots")]
        [SerializeField]
        protected Transform _weaponHandSlot;

        [SerializeField]
        protected Transform _weaponBackSlot;

        [Header("References")]
        [Tooltip(
            "Fallback: De onde o tiro sai se a arma não tiver o script RangedWeaponInstance configurado."
        )]
        [SerializeField]
        protected Transform _defaultMuzzlePoint;

        protected CharacterAimController _aimController;
        protected InputReader _inputReader;

        // Instâncias Dinâmicas
        protected GameObject _weaponInstance;
        protected RangedWeaponInstance _weaponInstanceRef;

        protected float _lastShootTime;
        
        [Header("Runtime - Ammo System")]
        [SerializeField]
        protected int currentClipAmmo;
        protected bool isReloading;

        protected virtual void Awake()
        {
            _aimController = GetComponent<CharacterAimController>();
            _inputReader = GetComponentInParent<InputReader>();
            if (_inputReader == null)
                _inputReader = FindFirstObjectByType<InputReader>();
        }

        protected virtual void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAimStarted += HandleAimStarted;
                _inputReader.OnAimCanceled += HandleAimCanceled;
                _inputReader.OnAttackPressed += HandleAttackPressed;
            }
        }

        protected virtual void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAimStarted -= HandleAimStarted;
                _inputReader.OnAimCanceled -= HandleAimCanceled;
                _inputReader.OnAttackPressed -= HandleAttackPressed;
            }
        }

        protected virtual void Start()
        {
            if (_currentWeapon != null)
            {
                EquipRangedWeapon(_currentWeapon);
            }
        }

        protected virtual bool CanAimOrShoot()
        {
            ICombatAddon combatAddon = GetComponent<ICombatAddon>();
            if (combatAddon != null && combatAddon.IsMeleeStance)
            {
                return false; // Bloqueado se a espada estiver puxada!
            }
            if (isReloading)
            {
                return false; // Bloqueado se estiver recarregando!
            }
            return true;
        }

        public virtual void HandleAimStarted()
        {
            if (!CanAimOrShoot())
                return;

            if (_weaponHandSlot == null || _weaponInstance == null)
                return;

            _weaponInstance.transform.SetParent(_weaponHandSlot, false);

            if (_zeroLocalPositionOnEquip)
            {
                _weaponInstance.transform.localPosition = Vector3.zero;
                _weaponInstance.transform.localRotation = Quaternion.identity;
            }
        }

        public virtual void HandleAimCanceled()
        {
            // Guarda a arma ao parar de mirar
            UnequipRangedWeapon();
        }

        // Atira ao APERTAR o gatilho, MAS apenas enquanto estiver mirando
        public virtual void HandleAttackPressed()
        {
            if (_currentWeapon == null || !CanAimOrShoot())
                return;

            if (_aimController.IsAiming)
            {
                TryShoot();
            }
        }

        public virtual void TryShoot()
        {
            if (Time.time < _lastShootTime + _currentWeapon.TimeBetweenShots)
                return;

            if (currentClipAmmo <= 0)
            {
                Debug.Log("<color=red>[Arma Sem Munição!]</color> Click! Click!");
                Reload();
                return;
            }

            _lastShootTime = Time.time;
            currentClipAmmo--;
            PerformShoot();
        }

        protected virtual void PerformShoot()
        {
            // --- Animação de Disparo ---
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Shoot");
            }

            Transform muzzle = _defaultMuzzlePoint;
            if (_weaponInstanceRef != null && _weaponInstanceRef.MuzzlePoint != null)
            {
                muzzle = _weaponInstanceRef.MuzzlePoint;
            }
            else if (muzzle == null)
            {
                muzzle = transform; // Fallback
            }

            // O VFX de Flash sai do Muzzle
            // TODO: Tocar Partícula de MuzzleFlash aqui.

            // --- HITSCAN LOGIC ---
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                // Dispara do centro exato da tela (Crosshair)
                Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _currentWeapon.HitMask))
                {
                    // Achou um alvo!
                    if (hit.collider.TryGetComponent<nakatimat.DamageSystem.IDamageable>(out var damageable))
                    {
                        damageable.ApplyDamage(_currentWeapon.BaseDamage, gameObject);
                        Debug.Log($"<color=orange>[Hitscan]</color> Acertou {hit.collider.name} com {_currentWeapon.BaseDamage} de dano!");
                    }
                    else
                    {
                        Debug.Log($"<color=grey>[Hitscan]</color> Bateu no cenário: {hit.collider.name}");
                    }
                }
            }
        }

        public virtual void Reload()
        {
            if (isReloading || currentClipAmmo >= _currentWeapon.ClipSize)
                return;

            Debug.Log($"<color=yellow>[Reloading...]</color> Trocando o pente. Duração: {_currentWeapon.ReloadTime}s");
            isReloading = true;
            
            // Invoca o término do reload usando delay simples. 
            // Num jogo real, isso deveria vir de um Animation Event do Animator.
            Invoke(nameof(FinishReload), _currentWeapon.ReloadTime);
        }

        protected virtual void FinishReload()
        {
            isReloading = false;
            currentClipAmmo = _currentWeapon.ClipSize;
            Debug.Log($"<color=green>[Reload Complete!]</color> Munição: {currentClipAmmo}/{_currentWeapon.ClipSize}");
        }

        public virtual void EquipRangedWeapon(RangedWeaponStats newWeapon)
        {
            if (newWeapon == null)
                return;
            _currentWeapon = newWeapon;

            // Enche o pente ao equipar a primeira vez (simplificado)
            currentClipAmmo = _currentWeapon.ClipSize;

            if (_weaponInstance != null)
                Destroy(_weaponInstance);
            
            _weaponInstanceRef = null;

            if (_currentWeapon.WeaponPrefab != null && _weaponBackSlot != null)
            {
                _weaponInstance = Instantiate(_currentWeapon.WeaponPrefab, _weaponBackSlot);
                if (_zeroLocalPositionOnEquip)
                {
                    _weaponInstance.transform.localPosition = Vector3.zero;
                    _weaponInstance.transform.localRotation = Quaternion.identity;
                }
                _weaponInstanceRef = _weaponInstance.GetComponent<RangedWeaponInstance>();
            }
        }

        public virtual void UnequipRangedWeapon()
        {
            // Cancela reload se guardar a arma
            if (isReloading)
            {
                CancelInvoke(nameof(FinishReload));
                isReloading = false;
                Debug.Log("<color=red>[Reload Cancelado]</color> Arma guardada antes de terminar.");
            }

            if (_weaponInstance != null && _weaponBackSlot != null)
            {
                _weaponInstance.transform.SetParent(_weaponBackSlot, false);
                if (_zeroLocalPositionOnEquip)
                {
                    _weaponInstance.transform.localPosition = Vector3.zero;
                    _weaponInstance.transform.localRotation = Quaternion.identity;
                }
            }
        }

        [Tooltip(
            "Se verdadeiro, o script zera a posição da arma quando equipa. Desative se o seu prefab já tem o offset correto salvo."
        )]
        [SerializeField]
        protected bool _zeroLocalPositionOnEquip = true;
    }
}
