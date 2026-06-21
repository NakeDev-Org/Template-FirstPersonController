using System;
using UnityEngine;

namespace nakatimat.DamageSystem
{
    public class CharacterHealthManager : MonoBehaviour, IDamageable
    {
        // Evento global: (Vitima, Dano Bruto, Dano Final)
        public static event Action<GameObject, float, float> OnAnyDamageTaken;

        // --- OBSERVER PATTERN (Eventos Locais para UI/Áudio) ---
        public event Action<float, float> OnHealthChanged; // (currentHP, maxHP)
        public event Action<float> OnDamageReceived; // (dano recebido)
        public event Action OnDeath;

        public TPSHealthStats TPSHealthStats;

        [SerializeField]
        protected float currentHP;
        protected IDefenseProvider _defenseProvider;

        protected virtual void Start()
        {
            currentHP = TPSHealthStats != null ? TPSHealthStats.maxHP : 100;
            _defenseProvider = GetComponent<IDefenseProvider>();
        }

        public virtual void ApplyDamage(float damageAmount, GameObject attacker = null)
        {
            float multiplier = 1f;

            if (_defenseProvider != null)
            {
                bool isParry = false;
                float defenseMultiplier = _defenseProvider.GetDefenseMultiplier(out isParry);
                multiplier *= defenseMultiplier;

                if (isParry && attacker != null)
                {
                    _defenseProvider.OnParrySuccess(attacker);
                }
            }

            float finalDamage = damageAmount * multiplier;

            // Notifica o Debugger e o mundo
            OnAnyDamageTaken?.Invoke(gameObject, damageAmount, finalDamage);
            OnDamageReceived?.Invoke(finalDamage);

            currentHP -= finalDamage;

            if (TPSHealthStats != null && currentHP > TPSHealthStats.maxHP)
            {
                currentHP = TPSHealthStats.maxHP;
            }

            OnHealthChanged?.Invoke(currentHP, TPSHealthStats != null ? TPSHealthStats.maxHP : 100);

            if (currentHP <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            OnDeath?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
