using UnityEngine;

namespace nakatimat.DamageSystem
{
    public interface IDamageable
    {
        public void ApplyDamage(
            float damageAmount,
            DamageType damageType = DamageType.Physical,
            GameObject attacker = null
        );
    }
}
