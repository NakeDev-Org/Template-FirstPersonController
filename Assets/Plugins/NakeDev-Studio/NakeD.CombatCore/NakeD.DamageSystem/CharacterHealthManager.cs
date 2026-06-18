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
        public event Action<float, DamageType> OnDamageReceived; // (dano recebido, tipo de dano)
        public event Action OnDeath;

        public TPSHealthStats TPSHealthStats;

        [SerializeField]
        private float currentHP;
        private IDefenseProvider _defenseProvider;

        private void Start()
        {
            currentHP = TPSHealthStats != null ? TPSHealthStats.maxHP : 100;
            _defenseProvider = GetComponent<IDefenseProvider>();
        }

        public void ApplyDamage(
            float damageAmount,
            DamageType damageType,
            GameObject attacker = null
        )
        {
            float multiplier = 1f;

            if (TPSHealthStats.TPSDamageResistances != null)
            {
                multiplier = TPSHealthStats.TPSDamageResistances.GetMultiplier(damageType);
            }

            if (_defenseProvider != null)
            {
                bool isParry = false;
                float defenseMultiplier = _defenseProvider.GetDefenseMultiplier(
                    damageType,
                    out isParry
                );
                multiplier *= defenseMultiplier;

                if (isParry && attacker != null)
                {
                    _defenseProvider.OnParrySuccess(attacker);
                }
            }

            float finalDamage = damageAmount * multiplier;

            // Notifica o Debugger e o mundo
            OnAnyDamageTaken?.Invoke(gameObject, damageAmount, finalDamage);
            OnDamageReceived?.Invoke(finalDamage, damageType);

            currentHP -= finalDamage;

            if (currentHP > TPSHealthStats.maxHP)
            {
                currentHP = TPSHealthStats.maxHP;
            }

            OnHealthChanged?.Invoke(currentHP, TPSHealthStats.maxHP);

            if (currentHP <= 0)
            {
                OnDeath?.Invoke();
                gameObject.SetActive(false);
            }
        }
    }
}
