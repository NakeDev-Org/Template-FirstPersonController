using UnityEngine;

namespace nakatimat.TPS.Player
{
    public class StaminaController : MonoBehaviour
    {
        [Header("Stamina Settings")]
        public float maxStamina = 100f;
        public float regenRate = 20f; // Quanto regenera por segundo
        public float regenDelay = 1.0f; // Tempo de espera após gastar para voltar a regenerar

        [Header("Debug")]
        [SerializeField]
        private bool showDebugLog = true;

        private float _currentStamina;
        private float _lastConsumeTime;

        private void Awake()
        {
            _currentStamina = maxStamina;
        }

        private void Update()
        {
            if (_currentStamina < maxStamina)
            {
                if (Time.time >= _lastConsumeTime + regenDelay)
                {
                    _currentStamina += regenRate * Time.deltaTime;
                    if (_currentStamina > maxStamina)
                    {
                        _currentStamina = maxStamina;
                        if (showDebugLog)
                            Debug.Log(
                                $"[STAMINA] Cheia: {_currentStamina}/{maxStamina}"
                            );
                    }
                }
            }
        }

        public bool HasEnoughStamina(float amount)
        {
            return _currentStamina >= amount;
        }

        public void ConsumeStamina(float amount)
        {
            _currentStamina -= amount;
            if (_currentStamina < 0f)
                _currentStamina = 0f;

            _lastConsumeTime = Time.time;

            if (showDebugLog)
            {
                Debug.Log(
                    $"[STAMINA] Gasto: {amount} | Atual: {_currentStamina:F1}/{maxStamina}"
                );
            }
        }
    }
}
