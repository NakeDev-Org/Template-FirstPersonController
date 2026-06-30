using UnityEngine;

namespace nakatimat.TPS.Player.Modular
{
    /// <summary>
    /// Adiciona um efeito de Sway (Inclinação no eixo Z) à câmera quando o jogador vira o mouse.
    /// Isso aumenta a sensação de peso e velocidade, complementando o WeaponSway das mãos.
    /// </summary>
    public class CameraSway : MonoBehaviour
    {
        [Header("Camera Sway Settings")]
        [Tooltip("Multiplicador da inclinação. Valores positivos fazem a câmera inclinar contra o movimento (sensação de peso).")]
        public float swayMultiplier = 1.5f;
        
        [Tooltip("Suavidade com que a câmera volta ao eixo reto.")]
        public float smooth = 10f;
        
        [Tooltip("Limite máximo de inclinação em graus (Eixo Z).")]
        public float maxTiltAngle = 3f;

        private InputReader _inputReader;
        private Camera _camera;

        private void Start()
        {
            _inputReader = GetComponentInParent<InputReader>();
            _camera = GetComponent<Camera>();
            if (_camera == null) _camera = GetComponentInChildren<Camera>();
        }

        private void LateUpdate()
        {
            if (_inputReader == null || _camera == null) return;

            // Pega o input horizontal do mouse
            float mouseX = _inputReader.LookInput.x;

            // Calcula o ângulo de inclinação (Roll no eixo Z) baseado na velocidade do mouse
            float targetTilt = Mathf.Clamp(-mouseX * swayMultiplier, -maxTiltAngle, maxTiltAngle);

            // A rotação da câmera no FPSAimAddon só usa X (Pitch). 
            // Então podemos ler a rotação atual, preservar o X e Y, e suavizar o Z!
            Vector3 currentEuler = _camera.transform.localEulerAngles;
            
            // Corrige o ângulo do Z para trabalhar com números negativos na Unity
            float currentZ = currentEuler.z > 180f ? currentEuler.z - 360f : currentEuler.z;

            // Suaviza a inclinação
            float newZ = Mathf.Lerp(currentZ, targetTilt, smooth * Time.deltaTime);

            // Aplica a nova rotação mantendo o Pitch que o FPSAimAddon acabou de calcular no LateUpdate
            _camera.transform.localEulerAngles = new Vector3(currentEuler.x, currentEuler.y, newZ);
        }
    }
}
