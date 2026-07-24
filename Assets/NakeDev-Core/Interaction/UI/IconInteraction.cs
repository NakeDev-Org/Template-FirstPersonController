using nakatimat.Core;
using UnityEngine;
using UnityEngine.UI;

namespace nakatimat.InteractionSystem.UI
{
    /// <summary>
    /// Ícone inteligente de interação. Ele mesmo calcula sua distância até o jogador e usa WorldToScreenPoint para se posicionar na tela (Screen Space).
    /// </summary>
    [DefaultExecutionOrder(100)] // Garante que a UI atualize DEPOIS que a Câmera (Cinemachine ou Script) já se moveu!
    public class IconInteraction : MonoBehaviour
    {
        [Header("Referências")]
        [Tooltip("A Imagem do Canvas que será alterada.")]
        public Image iconImage;

        [Header("Ícones")]
        [Tooltip("Conjunto de ícones (global/teclado/gamepad). Crie um asset via Create > NakeDev > Interaction > Icon Set e reutilize em qualquer interactable.")]
        public InteractionIconSetSO iconSet;

        [Header("Configurações")]
        [Tooltip("Distância máxima para mostrar o ícone na tela")]
        public float maxDistance = 5f;

        private Transform _playerTransform;
        private IInteractionInput _inputReader;
        private Camera _mainCamera;
        private Canvas _canvas;

        private bool _isTargeted = false;
        private InputDeviceType _lastDeviceType = InputDeviceType.Keyboard;

        public void Setup()
        {
            _mainCamera = Camera.main;
            
            // Tenta encontrar o player automaticamente (KISS)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
                _inputReader = player.GetComponentInChildren<IInteractionInput>();
            }

            _canvas = GetComponentInChildren<Canvas>();
            if (_canvas == null)
                _canvas = GetComponent<Canvas>();
        }

        public void SetTargeted(bool isTargeted)
        {
            _isTargeted = isTargeted;
            UpdateIconSprite();
        }

        private void UpdateIconSprite()
        {
            if (iconImage == null || iconSet == null)
                return;

            InputDeviceType deviceType = _inputReader != null ? _inputReader.CurrentDeviceType : InputDeviceType.Keyboard;
            iconImage.sprite = iconSet.GetIcon(_isTargeted, deviceType);

            // Evita que ícones de proporções diferentes fiquem espremidos/esticados
            iconImage.preserveAspect = true;
            iconImage.SetNativeSize();
        }

        void LateUpdate()
        {
            if (_playerTransform == null || _mainCamera == null || iconImage == null)
                return;
                
            // Monitora a troca de dispositivo (teclado/Xbox/PlayStation/Nintendo) em tempo real se estiver mirando
            if (_isTargeted && _inputReader != null)
            {
                if (_inputReader.CurrentDeviceType != _lastDeviceType)
                {
                    _lastDeviceType = _inputReader.CurrentDeviceType;
                    UpdateIconSprite();
                }
            }

            // Calcula a distância usando a própria posição deste objeto no mundo 3D
            float distance = Vector3.Distance(transform.position, _playerTransform.position);

            if (distance <= maxDistance)
            {
                // Verifica se está atrás da câmera
                bool isBehind = Vector3.Dot(transform.position - _mainCamera.transform.position, _mainCamera.transform.forward) < 0;

                if (!isBehind)
                {
                    // Liga a Imagem se estiver perto e na frente da câmera
                    if (!iconImage.enabled)
                    {
                        iconImage.enabled = true;
                        UpdateIconSprite();
                    }

                    // Move APENAS A IMAGEM para a posição na tela, usando a posição Deste Objeto como referência 3D
                    Vector2 screenPos = _mainCamera.WorldToScreenPoint(transform.position);
                    iconImage.transform.position = screenPos;
                }
                else
                {
                    // Se estiver atrás da câmera, esconde a imagem
                    if (iconImage.enabled)
                        iconImage.enabled = false;
                }
            }
            else
            {
                // Desliga a imagem se estiver longe
                if (iconImage.enabled)
                {
                    iconImage.enabled = false;
                    // Reseta o estado para garantir que não fique "preso" mirado
                    if (_isTargeted)
                        SetTargeted(false);
                }
            }
        }
    }
}
