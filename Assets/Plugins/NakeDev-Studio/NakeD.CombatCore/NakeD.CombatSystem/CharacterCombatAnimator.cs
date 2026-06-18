using System;
using System.Collections.Generic;
using nakatimat.AttackSystem;
using nakatimat.ComboFramework.Data;
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
        private int AttackSpeedMultiplierHash = Animator.StringToHash(
            "AttackSpeedMultiplier"
        );
        private int IsBlockingHash = Animator.StringToHash("IsBlocking");

        [Header("Runtime - Combo Graph")]
        private bool _isMelee;
        private ComboNode _currentComboNode;
        private bool _canQueueNextAttack = false;
        private bool _buffAttack;
        private AttackInputType _buffInputType; // Armazena a memória de qual botão foi pressionado
        private bool _isWeaponInHand;
        private float _attackTimeout;
        private bool _isHitboxOpen; // Evita acionar o Hit múltiplas vezes no mesmo nó

        private AttackData _attackData; // Dano Base Injetado
        private TPSMeleeWeaponStats _currentWeaponData;
        private HandIK _handIK;
        private ProceduralFootIK _footIK;

        private void Awake()
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

        private void Update()
        {
            // --- DATA-DRIVEN NORMALIZED TIME ENGINE ---
            if (_currentComboNode != null)
            {
                // Failsafe Aéreo Crítico (Se travar na animação, reseta)
                _attackTimeout -= Time.deltaTime;
                if (_attackTimeout <= 0f)
                {
                    ResetCombo();
                }

                AnimatorStateInfo stateInfo =
                    _animator.GetCurrentAnimatorStateInfo(0);

                // Só começamos a ler o tempo se a animação do nó realmente começou a tocar (evita ler na transição do Crossfade)
                if (stateInfo.IsName(_currentComboNode.animationStateName))
                {
                    float nTime = stateInfo.normalizedTime;

                    // 1. Lógica da Hitbox
                    if (
                        nTime >= _currentComboNode.hitboxStartTime
                        && nTime <= _currentComboNode.hitboxEndTime
                    )
                    {
                        if (!_isHitboxOpen)
                        {
                            _isHitboxOpen = true;
                            Hit(); // Dispara Efeitos e Dano
                        }
                    }
                    else if (
                        nTime > _currentComboNode.hitboxEndTime
                        && _isHitboxOpen
                    )
                    {
                        _isHitboxOpen = false;
                        // Futuramente: Chamar CloseHitbox() aqui para desativar o collider físico
                    }

                    // 2. Lógica da Janela de Combo
                    if (
                        nTime >= _currentComboNode.comboWindowStartTime
                        && nTime <= _currentComboNode.comboWindowEndTime
                    )
                    {
                        _canQueueNextAttack = true;

                        // Se o jogador já apertou o botão dentro da janela, disparamos imediatamente
                        if (_buffAttack)
                        {
                            PerformCombo();
                        }
                    }
                    else if (nTime > _currentComboNode.comboWindowEndTime)
                    {
                        _canQueueNextAttack = false;
                    }

                    // 3. Fim da Animação
                    if (nTime >= 0.98f)
                    {
                        ResetCombo();
                    }
                }
            }

            bool isAttacking = _buffAttack || _currentComboNode != null;

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
                    _handIK.DisableRightHandIK =
                        _currentWeaponData.disableRightHandIK;
                    _handIK.DisableLeftHandIK =
                        _currentWeaponData.disableLeftHandIK;
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

        public void EquipWeapon(TPSMeleeWeaponStats TPSMeleeWeaponStats)
        {
            if (TPSMeleeWeaponStats == null)
                return;

            _currentWeaponData = TPSMeleeWeaponStats;

            if (_attackEffect != null && TPSMeleeWeaponStats.comboGraph != null)
            {
                // TODO: Refactor TPSMeleeVFXController to read from ComboGraph instead of TPSComboStepDataBase
                // _attackEffect.SetAnimationDatabase(
                //     TPSMeleeWeaponStats.animationDatabase
                // );
            }

            bool previewIsMelee = _isMelee;
            _isMelee = false;
            _animator.SetBool(IsMeleeHash, _isMelee);

            if (TPSMeleeWeaponStats.animatorOverride != null)
            {
                _animator.runtimeAnimatorController =
                    TPSMeleeWeaponStats.animatorOverride;
            }
            else
            {
                Debug.LogWarning(
                    $"No animator override assigned on TPSMeleeWeaponStats: {TPSMeleeWeaponStats.name}"
                );
            }

            _animator.SetFloat(
                AttackSpeedMultiplierHash,
                TPSMeleeWeaponStats.AttackData.attackSpeedMultiplier
            );

            if (previewIsMelee == true)
            {
                EnterCombatMode();
            }
        }

        public void Attack(
            AttackData AttackData,
            AttackInputType inputType = AttackInputType.LightAttack
        )
        {
            if (AttackData == null)
                return;

            _attackData = AttackData;
            _buffInputType = inputType; // Salva o input para sabermos qual ramo seguir

            if (_isMelee == false)
            {
                // Hack and Slash Feel: Entra em modo combate instantaneamente
                _isMelee = true;
                _animator.SetBool(IsMeleeHash, _isMelee);

                // Força a malha da espada para a mão imediatamente (pula a animação de Draw)
                GetComponent<CharacterEquipmentManager>()?.Equip();
            }

            if (_isWeaponInHand == false)
            {
                GetComponent<CharacterEquipmentManager>()?.Equip();
            }

            _buffAttack = true;

            if (_currentComboNode == null)
            {
                PerformAttack(); // Ataque inicial da árvore
            }
            else if (_currentComboNode != null && _canQueueNextAttack == true)
            {
                PerformCombo(); // Ataque subsequente da ramificação
            }
        }

        private void PerformAttack()
        {
            if (
                _currentWeaponData == null
                || _currentWeaponData.comboGraph == null
                || _currentWeaponData.comboGraph.entryNode == null
            )
            {
                _buffAttack = false;
                return;
            }

            _buffAttack = false;
            _currentComboNode = _currentWeaponData.comboGraph.entryNode;

            _attackTimeout = 2.0f; // Failsafe máximo
            _isHitboxOpen = false;
            _canQueueNextAttack = false;

            _animator.SetBool(IsAttackingHash, true);
            // O CÓDIGO TOMA O CONTROLE: Injeta a animação ignorando setas do Animator
            _animator.CrossFadeInFixedTime(
                _currentComboNode.animationStateName,
                0.1f
            );

            _movementBlocker?.SetMovmentBlocked(true);
        }

        private void PerformCombo()
        {
            if (
                _currentComboNode == null
                || _currentComboNode.nextPossibleNodes == null
                || _currentComboNode.nextPossibleNodes.Count == 0
            )
            {
                // Fim do combo, a arma não tem próximos ataques
                return;
            }

            // Lê o botão que o jogador apertou e procura a ramificação correta
            ComboNode nextNode = null;
            foreach (var branch in _currentComboNode.nextPossibleNodes)
            {
                if (branch.requiredInput == _buffInputType)
                {
                    nextNode = branch.nextNode;
                    break;
                }
            }

            if (nextNode == null)
                return; // Se apertou um botão que não tem ramificação (ex: tentou Heavy, mas só tinha Light), ignora e para o combo.

            _buffAttack = false;
            _canQueueNextAttack = false;
            _isHitboxOpen = false;

            _currentComboNode = nextNode;
            _attackTimeout = 2.0f; // Reseta failsafe

            _animator.SetBool(IsAttackingHash, true);
            // Injeta a próxima animação
            _animator.CrossFadeInFixedTime(
                _currentComboNode.animationStateName,
                0.1f
            );

            _movementBlocker?.SetMovmentBlocked(true);
        }

        public void ResetCombo()
        {
            _canQueueNextAttack = false;
            _currentComboNode = null;
            _buffAttack = false;
            _isHitboxOpen = false;

            _animator.SetBool(IsAttackingHash, false);
            _movementBlocker?.SetMovmentBlocked(false);
        }

        public bool GetIsCombatMode()
        {
            return _isMelee;
        }

        public bool IsAttacking()
        {
            return _buffAttack || _currentComboNode != null;
        }

        public void EnterCombatMode()
        {
            _isMelee = true;
            _animator.SetBool(IsMeleeHash, _isMelee);
            _animator.ResetTrigger(TriggerUnequipHash);
            _animator.SetTrigger(TriggerEquipHash);
        }

        public void ExitCombatMode()
        {
            if (_isMelee == true)
            {
                _animator.ResetTrigger(TriggerEquipHash);
                _animator.SetTrigger(TriggerUnequipHash);
            }

            _isMelee = false;
            _animator.SetBool(IsMeleeHash, _isMelee);
        }

        public void SetWeaponInHand(bool inHand)
        {
            _isWeaponInHand = inHand;
        }

        public void SetBlocking(bool isBlocking)
        {
            if (_animator != null)
            {
                _animator.SetBool(IsBlockingHash, isBlocking);
            }
        }

        // Hit() não é mais ativado por Animation Event, e sim nativamente pelo Update (Normalized Time)
        private void Hit()
        {
            _attackEffect?.TriggerAttack(_currentComboNode, _attackData);
            OnWeaponSwing?.Invoke();
        }
    }
}
