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

            for (int i = 0; i < finalAttackData.damages.Count; i++)
            {
                DamageInstance instance = finalAttackData.damages[i];
                instance.amount *= damageMultiplier;
                finalAttackData.damages[i] = instance;
            }

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
