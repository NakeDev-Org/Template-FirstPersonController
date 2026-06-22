using System;
using nakatimat.Core.Interfaces;
using nakatimat.Core.Character;
using UnityEngine;
using nakatimat.Core.Inspector;

namespace nakatimat.TPS.Player.Modular
{
    [RequireComponent(typeof(InputReader))]
    public class PlayerCombatAddon : BaseCombatAddon
    {
        protected InputReader _inputReader;
        protected IAimingAddon _aimingAddon;

        protected virtual void Awake()
        {
            _inputReader = GetComponent<InputReader>();
            _aimingAddon = GetComponent<IAimingAddon>();
        }

        protected virtual void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAttackPressed += HandleAttackPressed;
                _inputReader.OnAimStarted += HandleAimStarted;
                _inputReader.OnAimCanceled += HandleAimCanceled;
            }
        }

        protected virtual void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAttackPressed -= HandleAttackPressed;
                _inputReader.OnAimStarted -= HandleAimStarted;
                _inputReader.OnAimCanceled -= HandleAimCanceled;
            }
        }

        protected override Ray GetShootRay()
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                return mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            }
            
            // Fallback
            return new Ray(_defaultMuzzlePoint != null ? _defaultMuzzlePoint.position : transform.position, transform.forward);
        }

        protected virtual void HandleAttackPressed()
        {
            if (IsMeleeStance)
            {
                if (!IsBlocking)
                {
                    OnMeleeAttackTriggered?.Invoke();
                }
            }
            else
            {
                // Can only shoot if aiming
                if (_aimingAddon != null && _aimingAddon.IsAiming)
                {
                    TryShoot();
                }
            }
        }
    }
}
