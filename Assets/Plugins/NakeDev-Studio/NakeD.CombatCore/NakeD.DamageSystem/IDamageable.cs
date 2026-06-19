using UnityEngine;

namespace nakatimat.DamageSystem
{
    public interface IDamageable
    {
        public void ApplyDamage(
            float damageAmount,
            GameObject attacker = null
        );
    }
}
