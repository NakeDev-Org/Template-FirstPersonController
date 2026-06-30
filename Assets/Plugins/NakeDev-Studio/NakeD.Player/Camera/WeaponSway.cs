using UnityEngine;

namespace nakatimat.TPS.Player.Modular
{
    /// <summary>
    /// Adiciona o efeito clássico de FPS de "Weapon Sway" (arrasto de peso) baseado no movimento do mouse.
    /// Pode ser adicionado independentemente em armas, lanternas ou mãos.
    /// </summary>
    public class WeaponSway : MonoBehaviour
    {
        [Header("Sway Settings")]
        [Tooltip("Multiplicador da força do arrasto. Valores negativos invertem a direção.")]
        public float swayMultiplier = 2f;
        
        [Tooltip("Suavidade com que o objeto volta ao centro.")]
        public float smooth = 8f;
        
        [Tooltip("Limite máximo de rotação local no eixo X e Y.")]
        public float maxSwayAngle = 10f;
        
        [Header("Modo Avançado")]
        [Tooltip("Marque isso se você colocar o script direto em um OSSO animado (ex: RightShoulder). O sway será somado em cima da animação!")]
        public bool isAnimatedBone = false;

        private InputReader _inputReader;
        private Quaternion _initialLocalRotation;
        private Quaternion _currentSwayOffset = Quaternion.identity;

        private void Start()
        {
            // Tenta pegar o InputReader deste objeto ou do Root do Player
            _inputReader = GetComponentInParent<InputReader>();
            _initialLocalRotation = transform.localRotation;
        }

        private void Update()
        {
            if (_inputReader == null) return;

            // Pega o input puro do mouse
            Vector2 lookInput = _inputReader.LookInput;

            // Calcula o quanto o objeto deve torcer (Sway)
            float mouseX = Mathf.Clamp(lookInput.x * swayMultiplier, -maxSwayAngle, maxSwayAngle);
            float mouseY = Mathf.Clamp(lookInput.y * swayMultiplier, -maxSwayAngle, maxSwayAngle);

            // X do mouse vira rotação no eixo Y (Esquerda/Direita). Y do mouse vira rotação no eixo X (Cima/Baixo).
            Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
            Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

            // Rotação alvo do Sway puro (sem a rotação inicial)
            Quaternion targetSway = rotationX * rotationY;

            if (isAnimatedBone)
            {
                // Se for um osso animado, calculamos apenas o OFFSET de torção no Update
                _currentSwayOffset = Quaternion.Slerp(_currentSwayOffset, targetSway, smooth * Time.deltaTime);
            }
            else
            {
                // Se for um objeto estático (como a raiz do modelo), aplicamos a rotação total baseada no initialRotation
                Quaternion targetRotation = _initialLocalRotation * targetSway;
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            // O LateUpdate roda DEPOIS do Animator. 
            // Então pegamos a pose da animação e apenas multiplicamos nossa torção (Sway) por cima dela!
            if (isAnimatedBone)
            {
                transform.localRotation = transform.localRotation * _currentSwayOffset;
            }
        }
    }
}
