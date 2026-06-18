using System.Collections.Generic;
using UnityEngine;

namespace nakatimat.DamageSystem
{
    [CreateAssetMenu(
        fileName = "TPSDamageResistances",
        menuName = "NakeCore/TPS/Combat/Damage/Damage Modifier Data"
    )]
    public class TPSDamageResistances : ScriptableObject
    {
        public List<Modifier> modifiers = new List<Modifier>();

        public float GetMultiplier(DamageType type)
        {
            foreach (var mod in modifiers)
            {
                if (mod.damageType == type)
                {
                    return mod.multiplier;
                }
            }
            return 1f; // padrao sem resistencia/fraqueza
        }
    }

    [System.Serializable]
    public struct Modifier
    {
        public DamageType damageType;

        [Range(-1f, 2f)]
        public float multiplier; // -1 (cura), 0 (imune), 1 (normal), 2 (dano dobrado)
    }
}
