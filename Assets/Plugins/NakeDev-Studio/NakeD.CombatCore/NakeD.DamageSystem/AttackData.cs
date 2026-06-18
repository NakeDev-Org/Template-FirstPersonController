using System.Collections.Generic;
using UnityEngine;

namespace nakatimat.DamageSystem
{
    [CreateAssetMenu(
        fileName = "AttackData",
        menuName = "NakeCore/TPS/Combat/Damage/Attack Data"
    )]
    public class AttackData : ScriptableObject
    {
        public List<DamageInstance> damages = new List<DamageInstance>();
        public LayerMask damageableLayer;

        [Header("Attack Speed")]
        public float attackSpeedMultiplier = 1;

        [Header("Critico (optional)")]
        public bool canCrit = false;

        [Range(0f, 1f)]
        public float critChance = 0.1f;
        public float critMultiplier = 1.5f;

        public float GetCriticalMultiplier()
        {
            if (canCrit == true)
            {
                float rool = Random.value;
                if (rool <= critChance)
                {
                    return critMultiplier;
                }
            }

            return 1f;
        }
    }

    [System.Serializable]
    public struct DamageInstance
    {
        public float amount;
        public DamageType damageType;
    }
}
