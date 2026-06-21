using UnityEngine;

namespace nakatimat.DamageSystem
{
    [CreateAssetMenu(
        fileName = "AttackData",
        menuName = "NakeD/Combat/Attack Data"
    )]
    public class AttackData : ScriptableObject
    {
        [Header("Dano Bruto (Survival Horror)")]
        [Tooltip("Dano base infligido no alvo")]
        public float baseDamage;
        
        [Tooltip("Força de impacto para efeitos de Knockback (se necessário)")]
        public float impactForce = 1f;

        public LayerMask damageableLayer;

        [Header("Attack Speed")]
        public float attackSpeedMultiplier = 1;
    }
}
