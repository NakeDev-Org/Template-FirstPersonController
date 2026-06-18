using nakatimat.AttackSystem;

using nakatimat.DamageSystem;
using UnityEngine;

namespace nakatimat.CombatSystem.MeleeSystem
{
    [CreateAssetMenu(
        fileName = "Weapon Data",
        menuName = "NakeCore/TPS/Combat/Weapons/Weapon Data"
    )]
    public class TPSMeleeWeaponStats : ScriptableObject
    {
        public GameObject prefab;

        [Tooltip(
            "O Dano Base da arma. Será multiplicado pelo nó atual do combo."
        )]
        public AttackData AttackData;

        [Header("Animation & Combat Graph")]
        public RuntimeAnimatorController animatorOverride;



        [Header("Defense Settings")]
        public BlockStats BlockStats;

        [Header("IK Settings")]
        public bool disableRightHandIK = true;
        public bool disableLeftHandIK = false;
    }
}
