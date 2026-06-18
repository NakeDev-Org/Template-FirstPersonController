using nakatimat.CombatSystem.MeleeSystem;
using nakatimat.Core.Interfaces;
using nakatimat.DamageSystem;
using UnityEngine;

namespace nakatimat.TPS.Player.Modular
{
    [RequireComponent(typeof(CharacterEquipmentManager))]
    [RequireComponent(typeof(StaminaController))]
    public class PlayerCombatManager
        : MonoBehaviour,
            ICombatAddon,
            IDefenseProvider
    {
        [Header("Dependencies")]
        [SerializeField]
        private InputReader _inputReader;

        [Header("Defense & Parry Settings")]
        [Tooltip(
            "Tempo em segundos após levantar a guarda onde o Parry é ativado."
        )]
        public float parryWindow = 0.25f;

        [Tooltip(
            "Multiplicador de dano ao defender normalmente. 0 = bloqueia tudo, 0.5 = toma metade."
        )]
        public float blockDamageMultiplier = 0f;

        [Tooltip("Custo de Stamina para receber um golpe defendendo.")]
        public float blockStaminaCost = 15f;

        // Modules that might exist on the player
        private CharacterEquipmentManager _weaponHandler;
        private StaminaController _staminaController;
        private TargetingSystem _targetingSystem;

        public bool IsMeleeStance { get; private set; }
        public bool IsBlocking { get; private set; }
        private float _blockStartTime;

        private void Awake()
        {
            if (_inputReader == null)
                _inputReader = GetComponent<InputReader>();
            _weaponHandler = GetComponent<CharacterEquipmentManager>();
            _staminaController = GetComponent<StaminaController>();
            _targetingSystem = GetComponent<TargetingSystem>();
        }

        private void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAttackPressed += HandleAttack;
                _inputReader.OnWeaponToggled += HandleWeaponToggle;
                _inputReader.OnBlockStarted += HandleBlockStart;
                _inputReader.OnBlockCanceled += HandleBlockStop;
            }
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAttackPressed -= HandleAttack;
                _inputReader.OnWeaponToggled -= HandleWeaponToggle;
                _inputReader.OnBlockStarted -= HandleBlockStart;
                _inputReader.OnBlockCanceled -= HandleBlockStop;
            }
        }

        private void Update()
        {
            if (_weaponHandler != null)
            {
                IsMeleeStance = _weaponHandler.GetIsCombatMelee();
                IsBlocking = _weaponHandler.IsBlocking();
            }
        }

        private void HandleAttack()
        {
            IAimingAddon aimingAddon = GetComponent<IAimingAddon>();
            if (aimingAddon != null && aimingAddon.IsAiming)
            {
                return; // Bloqueia atacar (ou sacar a espada ao tentar atacar) se estiver mirando!
            }

            PlayerLocomotion locomotion = GetComponent<PlayerLocomotion>();
            if (locomotion != null && !locomotion.IsGrounded)
            {
                return; // Bloqueia atacar enquanto estiver no ar (evita o bug de animação)!
            }

            if (!IsMeleeStance)
            {
                // Just equip the weapon if not in melee stance
                _weaponHandler?.Attack();
                return;
            }

            if (_staminaController != null && _weaponHandler != null)
            {
                float attackCost = _weaponHandler.GetWeaponStaminaCost();
                if (!TryConsumeStamina(attackCost))
                {
                    return; // Not enough stamina
                }
            }

            _weaponHandler?.Attack();
        }

        private void HandleWeaponToggle()
        {
            if (_weaponHandler != null && _weaponHandler.IsAttacking())
            {
                return; // Bloqueia guardar a arma enquanto estiver atacando!
            }

            IAimingAddon aimingAddon = GetComponent<IAimingAddon>();
            if (aimingAddon != null && aimingAddon.IsAiming)
            {
                return; // Bloqueia sacar a espada se estiver mirando!
            }

            _weaponHandler?.ToggleCombatMode();
        }

        private void HandleBlockStart()
        {
            _blockStartTime = Time.time;
            _weaponHandler?.StartBlock();

            // Z-Targeting inteligente: Só trava se tiver alguém bem perto e na frente da câmera!
            if (_targetingSystem != null)
            {
                _targetingSystem.SetTargeting(true, true);
            }
        }

        private void HandleBlockStop()
        {
            _weaponHandler?.StopBlock();

            // Solta a mira ao soltar a defesa
            if (_targetingSystem != null)
            {
                _targetingSystem.SetTargeting(false);
            }
        }

        public bool TryConsumeStamina(float amount)
        {
            if (
                _staminaController == null
                || !_staminaController.isActiveAndEnabled
            )
                return true; // If no stamina system or disabled, it's free

            if (_staminaController.HasEnoughStamina(amount))
            {
                _staminaController.ConsumeStamina(amount);
                return true;
            }
            return false;
        }

        public bool HasEnoughStamina(float amount)
        {
            if (
                _staminaController == null
                || !_staminaController.isActiveAndEnabled
            )
                return true;
            return _staminaController.HasEnoughStamina(amount);
        }

        // ==========================================
        // DEFENSE & PARRY (IDefenseProvider)
        // ==========================================
        public float GetDefenseMultiplier(
            DamageType damageType,
            out bool parrySuccess
        )
        {
            parrySuccess = false;

            if (!IsBlocking)
            {
                return 1f; // Tomar dano integral
            }

            // Verifica janela de Parry
            if (Time.time - _blockStartTime <= parryWindow)
            {
                parrySuccess = true;
                return 0f; // Parry anula todo o dano!
            }

            // Se for defesa normal, tenta gastar stamina.
            if (TryConsumeStamina(blockStaminaCost))
            {
                return blockDamageMultiplier; // Defesa com sucesso
            }

            // Se não tem stamina, a defesa "quebra" e toma o dano inteiro.
            return 1f;
        }

        public void OnParrySuccess(GameObject attacker)
        {
            Debug.Log(
                $"<color=cyan><b>PERFECT PARRY!</b></color> Você defletiu o ataque de {attacker.name}!"
            );

            // Aqui você pode tocar som, instanciar VFX de faísca,
            // Ou até tentar achar um IDamageable no 'attacker' para aplicar Stun!
        }
    }
}
