using nakatimat.DamageSystem;
using UnityEngine;

namespace nakatimat.AttackSystem
{
    [RequireComponent(typeof(MeleeHitboxCaster))]
    public class TPSMeleeVFXController : MonoBehaviour
    {
        [Header("Ref")]
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
            AttackData baseAttackData,
            float damageMultiplier = 1f
        )
        {
            if (baseAttackData == null)
            {
                return;
            }

            PlayVFX();

            AttackData finalAttackData = Instantiate(baseAttackData);
            finalAttackData.baseDamage *= damageMultiplier;

            _attackController.Attack(
                hitBoxCenter,
                hitBoxSize,
                finalAttackData
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
