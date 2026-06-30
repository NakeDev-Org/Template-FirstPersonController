using nakatimat.DamageSystem;
using UnityEngine;
using nakatimat.Core.Inspector;

namespace nakatimat.AttackSystem
{
    [RequireComponent(typeof(MeleeHitboxCaster))]
    public class TPSMeleeVFXController : MonoBehaviour
    {
        [Separator("Ref", 150, 150, 150)]
        private MeleeHitboxCaster _attackController;

        [SerializeField]
        private ParticleSystem _slashVFX;

        private void Awake()
        {
            _attackController = GetComponent<MeleeHitboxCaster>();
        }

        public void TriggerAttack(
            Vector3 hitBoxCenter,
            Vector3 hitBoxSize,
            float baseDamage,
            LayerMask damageableLayer
        )
        {
            PlayVFX();

            _attackController.Attack(
                hitBoxCenter,
                hitBoxSize,
                baseDamage,
                damageableLayer
            );
        }

        private void PlayVFX()
        {
            if (_slashVFX == null)
                return;

            _slashVFX.Play();
        }
    }
}
