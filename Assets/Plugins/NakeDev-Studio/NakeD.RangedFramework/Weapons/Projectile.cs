using UnityEngine;
using nakatimat.DamageSystem;

namespace nakatimat.RangedFramework
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Tempo até o projétil ser destruído automaticamente.")]
        public float Lifetime = 5f;

        [HideInInspector] public float Damage;
        [HideInInspector] public GameObject Instigator;
        [HideInInspector] public LayerMask HitMask;

        private void Start()
        {
            // Limpa o objeto após um tempo (evita memory leak)
            Destroy(gameObject, Lifetime);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Ignorar o próprio Instigator
            if (other.gameObject == Instigator) return;

            // Verificar se a layer está na HitMask
            if (((1 << other.gameObject.layer) & HitMask) != 0)
            {
                if (other.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.ApplyDamage(Damage, Instigator);
                }
                
                // Opcional: Instanciar VFX de impacto aqui
                Destroy(gameObject);
            }
        }
    }
}
