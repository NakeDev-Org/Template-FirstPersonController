using UnityEngine;
using UnityEngine.Events;
using nakatimat.Core.Inspector;

namespace nakatimat.Player
{
    [RequireComponent(typeof(InputReader))]
    public class PlayerFlashlight : MonoBehaviour
    {
        [Tooltip("Arraste a luz (Light) que servirá como lanterna.")]
        [SerializeField] private Light _flashlight;
        
        [Tooltip("Se verdadeiro, o jogo começa com a luz ligada.")]
        [SerializeField] private bool _startEnabled = false;

        [InspectorLine("Outputs (Regra 4)", 255, 150, 50)]
        [Tooltip("Disparado sempre que a lanterna acende ou apaga. Útil para tocar sons de clique.")]
        public UnityEvent OnFlashlightToggled;

        private InputReader _inputReader;
        private bool _isFlashlightOn;

        private void Awake()
        {
            _inputReader = GetComponent<InputReader>();

            _isFlashlightOn = _startEnabled;
            if (_flashlight != null)
            {
                _flashlight.enabled = _isFlashlightOn;
            }
        }

        private void OnEnable()
        {
            if (_inputReader != null)
            {
                // Usando o evento OnFlashlightToggled como botão da lanterna.
                _inputReader.OnFlashlightToggled += ToggleFlashlight;
            }
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnFlashlightToggled -= ToggleFlashlight;
            }
        }

        private void ToggleFlashlight()
        {
            if (_flashlight == null) return;

            _isFlashlightOn = !_isFlashlightOn;
            _flashlight.enabled = _isFlashlightOn;
            
            // Dispara o evento visual/sonoro para o Level Designer plugar o AudioSource
            OnFlashlightToggled?.Invoke();
        }
    }
}

