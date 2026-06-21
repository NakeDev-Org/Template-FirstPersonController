using UnityEngine;

namespace nakatimat.DamageSystem
{
    [CreateAssetMenu(
        fileName = "TPSHealthStats",
        menuName = "NakeD/Combat/Health Stats"
    )]
    public class TPSHealthStats : ScriptableObject
    {
        public int maxHP = 100;
    }
}
