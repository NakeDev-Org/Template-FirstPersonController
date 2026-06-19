using UnityEngine;

namespace nakatimat.DamageSystem
{
    [CreateAssetMenu(
        fileName = "TPSHealthStats",
        menuName = "NakeCore/TPS/Combat/Damage/Damageable Data"
    )]
    public class TPSHealthStats : ScriptableObject
    {
        public int maxHP = 100;
    }
}
