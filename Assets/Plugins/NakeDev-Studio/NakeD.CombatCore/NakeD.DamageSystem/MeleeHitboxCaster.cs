using UnityEngine;

namespace nakatimat.DamageSystem
{
    public class MeleeHitboxCaster : MonoBehaviour
    {
        // --- OBSERVER PATTERN (Eventos) ---
        public event System.Action<Vector3> OnEnemyHit;

        public void Attack(Vector3 center, Vector3 boxSize, AttackData AttackData)
        {
            Vector3 boxCenter = transform.position + transform.rotation * center;
            Vector3 halfExtents = boxSize * 0.5f;
            Collider[] hits = Physics.OverlapBox(
                boxCenter,
                halfExtents,
                transform.rotation,
                AttackData.damageableLayer
            );

#if UNITY_EDITOR
            // Envia para o Debugger Mundial ao invés de gerenciar localmente
            if (nakatimat.Core.DebugTools.GlobalDebugger.Instance != null)
            {
                nakatimat.Core.DebugTools.GlobalDebugger.Instance.RegisterHitbox(
                    boxCenter,
                    halfExtents,
                    transform.rotation,
                    1.5f
                );
            }
#endif

            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    if (AttackData != null)
                    {
                        foreach (var damage in AttackData.damages)
                        {
                            float multiplier = AttackData.GetCriticalMultiplier();
                            float finalDamage = damage.amount * multiplier;
                            damageable.ApplyDamage(
                                finalDamage,
                                damage.damageType,
                                transform.root.gameObject
                            );

                            OnEnemyHit?.Invoke(hit.ClosestPoint(boxCenter));
                        }
                    }
                }
            }
        }
    }
}
