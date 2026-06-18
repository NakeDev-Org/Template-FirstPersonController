using nakatimat.DamageSystem;
using UnityEngine;

namespace nakatimat.RangedFramework
{
    public class RangedProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [Tooltip(
            "Tempo em segundos até o projétil se destruir automaticamente caso não bata em nada."
        )]
        [SerializeField]
        private float _lifeTime = 5f;

        private float _damage;
        private Rigidbody _rb;
        private Collider _col;
        private bool _hasHit;
        private Vector3 _direction;
        private float _speed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<Collider>();

            // Se autodestrói após X segundos para não lotar a Hierarchy
            Destroy(gameObject, _lifeTime);
        }

        // Chamado pelo CharacterRangedCombat na hora do tiro
        public void Initialize(float damage, Vector3 direction, float speed)
        {
            _damage = damage;
            _direction = direction;
            _speed = speed;
            _hasHit = false;

            // Se tiver Rigidbody, usa a física da Unity (Bullet drop, gravidade, etc)
            if (_rb != null)
            {
                _rb.linearVelocity = _direction * _speed;
            }
        }

        private void Update()
        {
            // Se não tiver Rigidbody (Hitscan rápido ou magia reta), nós mesmos movemos ele!
            if (_rb == null && !_hasHit)
            {
                transform.position += _direction * _speed * Time.deltaTime;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_hasHit)
                return;

            // Se bater em algo que tem IDamageable
            if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.ApplyDamage(_damage);
            }

            _hasHit = true;

            // Trava o projétil no objeto (efeito de flecha cravada)
            if (TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
            }

            // Gruda no transform que colidiu
            transform.SetParent(collision.transform);

            // Opcional: desliga o collider para não atrapalhar a física depois de cravado
            if (TryGetComponent<Collider>(out Collider col))
            {
                col.enabled = false;
            }

            // Destrói depois de alguns segundos preso
            Destroy(gameObject, 5f);
        }
    }
}
