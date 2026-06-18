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
            "Arraste o ScriptableObject com as informações da sua arma (Arco ou Arma de Fogo)"
        )]
        [SerializeField]
        private RangedWeaponStats _currentWeapon;

        [Header("Equipment Slots")]
        [Tooltip(
            "Oso da Mão (Ex: WeaponSpawnHandSlot) onde o arco fica quando sacado."
        )]
        [SerializeField]
        private Transform _weaponHandSlot;

        [Tooltip(
            "Oso das Costas (Ex: Spine/BackSlot) onde o arco e a aljava ficam guardados."
        )]
        [SerializeField]
        private Transform _weaponBackSlot;

        [Header("References")]
        [Tooltip(
            "Fallback: De onde o tiro sai se a arma não tiver o script RangedWeaponInstance configurado."
        )]
        [SerializeField]
        private Transform _defaultMuzzlePoint;

        private CharacterAimController _aimController;
        private InputReader _inputReader;

        // Instâncias Dinâmicas
        private GameObject _weaponInstance;
        private GameObject _quiverInstance;
        private RangedWeaponInstance _weaponInstanceRef;

        private float _lastShootTime;

        private void Awake()
        {
            _aimController = GetComponent<CharacterAimController>();
            _inputReader = GetComponentInParent<InputReader>();
            if (_inputReader == null)
                _inputReader = FindFirstObjectByType<InputReader>();
        }

        private void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAimStarted += HandleAimStarted;
                _inputReader.OnAimCanceled += HandleAimCanceled;
                _inputReader.OnAttackPressed += HandleAttackPressed;
            }
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAimStarted -= HandleAimStarted;
                _inputReader.OnAimCanceled -= HandleAimCanceled;
                _inputReader.OnAttackPressed -= HandleAttackPressed;
            }
        }

        private bool CanAimOrShoot()
        {
            ICombatAddon combatAddon = GetComponent<ICombatAddon>();
            if (combatAddon != null && combatAddon.IsMeleeStance)
            {
                return false; // Bloqueado se a espada estiver puxada!
            }
            return true;
        }

        // Caso 1: Arco e Flecha (Atira ao SOLTAR o botão de mirar)
        public void HandleAimCanceled()
        {
            if (!CanAimOrShoot())
                return;

            if (
                _currentWeapon != null
                && _currentWeapon.WeaponType == RangedWeaponType.Bow
            )
            {
                TryShoot();
            }

            // Guarda o arco/arma ao parar de mirar
            UnequipRangedWeapon();
        }

        // Caso 2: Arma de Fogo (Atira ao APERTAR o gatilho, MAS apenas enquanto estiver mirando)
        public void HandleAttackPressed()
        {
            if (
                _currentWeapon == null
                || _currentWeapon.WeaponType != RangedWeaponType.Firearm
            )
                return;

            if (_aimController.IsAiming)
            {
                TryShoot();
            }
        }

        public void TryShoot()
        {
            if (Time.time < _lastShootTime + _currentWeapon.TimeBetweenShots)
                return;

            _lastShootTime = Time.time;
            PerformShoot();
        }

        private void PerformShoot()
        {
            if (_currentWeapon.ProjectilePrefab == null)
            {
                Debug.LogWarning(
                    "[RangedFramework] Tentou atirar, mas não há prefab de projétil configurado no RangedWeaponStats!"
                );
                return;
            }

            // --- Animação de Disparo ---
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Shoot");
            }

            Transform muzzle = null;
            if (
                _weaponInstanceRef != null
                && _weaponInstanceRef.MuzzlePoint != null
            )
            {
                muzzle = _weaponInstanceRef.MuzzlePoint;
            }
            else if (_defaultMuzzlePoint != null)
            {
                muzzle = _defaultMuzzlePoint;
            }
            else
            {
                muzzle = transform; // Fallback
            }

            Vector3 shootDirection = GetShootDirection(muzzle.position);

            GameObject projectile = Instantiate(
                _currentWeapon.ProjectilePrefab,
                muzzle.position,
                Quaternion.LookRotation(shootDirection)
            );

            // Passa o dano, direção e velocidade para o nosso script de projétil cuidar de tudo
            if (
                projectile.TryGetComponent<RangedProjectile>(out var archerProj)
            )
            {
                archerProj.Initialize(
                    _currentWeapon.ProjectileDamage,
                    shootDirection,
                    _currentWeapon.ProjectileSpeed
                );
            }

            // Opcional: Feedback visual e sonoro aqui
        }

        public void EquipRangedWeapon(RangedWeaponStats newWeapon)
        {
            if (newWeapon == null)
                return;
            _currentWeapon = newWeapon;

            // 1. Limpa instâncias antigas
            if (_weaponInstance != null)
                Destroy(_weaponInstance);
            if (_quiverInstance != null)
                Destroy(_quiverInstance);
            _weaponInstanceRef = null;

            // 2. Instancia o Quiver (Aljava) nas costas de forma permanente
            if (_currentWeapon.QuiverPrefab != null && _weaponBackSlot != null)
            {
                _quiverInstance = Instantiate(
                    _currentWeapon.QuiverPrefab,
                    _weaponBackSlot
                );
                _quiverInstance.transform.localPosition = Vector3.zero;
                _quiverInstance.transform.localRotation = Quaternion.identity;
            }

            // 3. Instancia a Arma nas costas por padrão (Guardada)
            if (_currentWeapon.WeaponPrefab != null && _weaponBackSlot != null)
            {
                _weaponInstance = Instantiate(
                    _currentWeapon.WeaponPrefab,
                    _weaponBackSlot
                );
                if (_zeroLocalPositionOnEquip)
                {
                    _weaponInstance.transform.localPosition = Vector3.zero;
                    _weaponInstance.transform.localRotation =
                        Quaternion.identity;
                }
                _weaponInstanceRef =
                    _weaponInstance.GetComponent<RangedWeaponInstance>();
            }
        }

        public void UnequipRangedWeapon()
        {
            // Move a arma de volta pras costas e zera a rotação local
            if (_weaponInstance != null && _weaponBackSlot != null)
            {
                _weaponInstance.transform.SetParent(_weaponBackSlot, false);
                if (_zeroLocalPositionOnEquip)
                {
                    _weaponInstance.transform.localPosition = Vector3.zero;
                    _weaponInstance.transform.localRotation =
                        Quaternion.identity;
                }
            }
        }

        [Tooltip(
            "Se verdadeiro, o script zera a posição do arco quando equipa. Desative se o seu prefab já tem o offset correto salvo."
        )]
        [SerializeField]
        private bool _zeroLocalPositionOnEquip = true;

        public void HandleAimStarted()
        {
            if (!CanAimOrShoot())
                return;

            if (_weaponHandSlot == null)
            {
                Debug.LogError(
                    "[RangedFramework] O campo 'Weapon Hand Slot' está VAZIO no Inspector! O arco não sabe para onde ir!"
                );
                return;
            }

            if (_weaponInstance == null)
            {
                Debug.LogError(
                    "[RangedFramework] A arma não foi instanciada! Não tem como mover para a mão."
                );
                return;
            }

            // O SEGredo: false faz ele NÃO tentar manter a posição do mundo (ficar nas costas)
            // Ele vai "pular" para o novo pai mantendo o offset local.
            _weaponInstance.transform.SetParent(_weaponHandSlot, false);

            if (_zeroLocalPositionOnEquip)
            {
                _weaponInstance.transform.localPosition = Vector3.zero;
                _weaponInstance.transform.localRotation = Quaternion.identity;
            }
        }

        private void Start()
        {
            // Inicializa a arma se já houver uma configurada no Inspector
            if (_currentWeapon != null)
            {
                EquipRangedWeapon(_currentWeapon);
            }
        }

        private Vector3 GetShootDirection(Vector3 muzzlePosition)
        {
            // O ideal para jogos TPS é traçar um Raycast do centro da Câmera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Ray ray = mainCam.ViewportPointToRay(
                    new Vector3(0.5f, 0.5f, 0f)
                );
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
                {
                    return (hit.point - muzzlePosition).normalized;
                }
                return ray.direction;
            }
            return transform.forward;
        }
    }
}
