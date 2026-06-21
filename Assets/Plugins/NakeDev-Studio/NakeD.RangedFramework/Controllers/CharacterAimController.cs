using nakatimat.Core.Interfaces;
using nakatimat.TPS.Player.Modular;
using UnityEngine;

namespace nakatimat.RangedFramework
{
    public class CharacterAimController : MonoBehaviour, IAimingAddon
    {
        private InputReader _inputReader;

        public bool IsAiming { get; private set; }

        private void Awake()
        {
            _inputReader = GetComponent<InputReader>();
        }

        private void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAimStarted += HandleAimStarted;
                _inputReader.OnAimCanceled += HandleAimCanceled;
            }
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAimStarted -= HandleAimStarted;
                _inputReader.OnAimCanceled -= HandleAimCanceled;
            }
        }

        public void HandleAimStarted()
        {
            ICombatAddon combatAddon = GetComponent<ICombatAddon>();
            if (combatAddon != null && combatAddon.IsMeleeStance)
            {
                return; // Bloqueia mirar se a espada estiver sacada!
            }

            IsAiming = true;
        }

        public void HandleAimCanceled()
        {
            IsAiming = false;
        }
    }
}
