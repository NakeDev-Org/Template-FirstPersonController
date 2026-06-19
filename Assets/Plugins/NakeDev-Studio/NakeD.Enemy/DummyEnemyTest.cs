using nakatimat.DamageSystem;
using UnityEngine;

namespace nakatimat.Enemy
{
    public class DummyEnemyTest : MonoBehaviour, IDamageable
    {
        public void ApplyDamage(float damageAmount, GameObject attacker = null)
        {
            Debug.Log(
                $"<color=orange>[DummyEnemyTest]</color> Tomei {damageAmount} de dano do atacante {attacker}!"
            );
        }
    }
}
