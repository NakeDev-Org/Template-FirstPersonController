using nakatimat.ComboFramework.Data;
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
            ComboNode comboNode,
            AttackData baseAttackData
        )
        {
            if (comboNode == null || baseAttackData == null)
            {
                return;
            }

            PlayVFX(comboNode);

            // Calcula o dano final (Dano da Arma X Multiplicador do Golpe)
            AttackData finalAttackData = Instantiate(baseAttackData);

            for (int i = 0; i < finalAttackData.damages.Count; i++)
            {
                DamageInstance instance = finalAttackData.damages[i];
                instance.amount *= comboNode.damageMultiplier;
                finalAttackData.damages[i] = instance;
            }

            _attackController.Attack(
                comboNode.hitBoxCenter,
                comboNode.hitBoxSize,
                finalAttackData
            );
        }

        private void PlayVFX(ComboNode comboNode)
        {
            if (_slashVFX == null)
                return;

            _slashVFX.transform.localPosition = comboNode.vfxPosition;
            _slashVFX.transform.localRotation = Quaternion.Euler(
                comboNode.vfxRotation
            );
            _slashVFX.transform.localScale = Vector3.one * comboNode.vfxScale;

            var mainModule = _slashVFX.main;
            mainModule.startColor = comboNode.vfxColor;

            _slashVFX.Play();
        }
    }
}
