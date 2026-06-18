using UnityEngine;

namespace nakatimat.TPS.Player.Modular
{
    public class TPSCameraAimController : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("O InputReader do jogador.")]
        [SerializeField]
        private InputReader _inputReader;

        [Tooltip("O objeto vazio (ex: CameraTarget) que o Cinemachine deve seguir/olhar.")]
        [SerializeField]
        private Transform _cameraTarget;

        [Header("Sensibilidade da Câmera")]
        public Vector2 mouseSensitivity = new Vector2(2f, 2f);
        public Vector2 gamepadSensitivity = new Vector2(150f, 150f);

        [Header("Restrições de Ângulo (Clamp)")]
        [Tooltip("O quanto a câmera pode olhar para baixo (valores negativos).")]
        public float bottomClamp = -60f;

        [Tooltip("O quanto a câmera pode olhar para cima (valores positivos).")]
        public float topClamp = 60f;

        [Header("Suavização (Smoothing)")]
        [Tooltip("Ativa a inércia/suavização da câmera (aceleração e frenagem).")]
        public bool enableSmoothing = true;

        [Tooltip(
            "Tempo que leva para a câmera atingir a velocidade máxima ou parar. Quanto menor, mais rápido."
        )]
        [Range(0.01f, 0.5f)]
        public float smoothTime = 0.05f;

        private float _cinemachineTargetPitch;
        private float _cinemachineTargetYaw;
        private Vector2 _currentLookInput;
        private Vector2 _lookInputVelocity;
        private bool _wasLockedOn;

        private TargetingSystem _targetingSystem;

        private void Awake()
        {
            if (_inputReader == null)
            {
                _inputReader = GetComponent<InputReader>();
            }
            _targetingSystem = GetComponent<TargetingSystem>();
        }

        private void Start()
        {
            if (_cameraTarget != null)
            {
                _cinemachineTargetYaw = _cameraTarget.rotation.eulerAngles.y;
            }
            else
            {
                Debug.LogWarning(
                    "TPSCameraAimController: _cameraTarget não foi assinalado! A câmera não vai rotacionar.",
                    this
                );
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void CameraRotation()
        {
            if (_inputReader == null || _cameraTarget == null)
                return;

            bool hasTarget =
                _targetingSystem != null && _targetingSystem.GetCurrentTarget() != null;

            if (hasTarget)
            {
                // Deixa a Target Camera (Cinemachine) assumir 100% do controle orbital.
                _wasLockedOn = true;
                return;
            }

            // A Mágica do Anti-Snap ao SAIR do Lock-On:
            if (!hasTarget && _wasLockedOn)
            {
                if (Camera.main != null)
                {
                    Vector3 camEuler = Camera.main.transform.eulerAngles;
                    _cinemachineTargetYaw = camEuler.y;

                    float pitch = camEuler.x;
                    if (pitch > 180f)
                        pitch -= 360f;
                    _cinemachineTargetPitch = -pitch;
                }
                _wasLockedOn = false;
            }

            Vector2 targetLookDelta = _inputReader.LookInput;

            if (enableSmoothing)
            {
                _currentLookInput = Vector2.SmoothDamp(
                    _currentLookInput,
                    targetLookDelta,
                    ref _lookInputVelocity,
                    smoothTime
                );
            }
            else
            {
                _currentLookInput = targetLookDelta;
            }

            Vector2 lookDelta = _currentLookInput;
            bool isGamepad = _inputReader.IsGamepad;

            float sensitivityX = isGamepad
                ? gamepadSensitivity.x * Time.deltaTime
                : mouseSensitivity.x * 0.1f;
            float sensitivityY = isGamepad
                ? gamepadSensitivity.y * Time.deltaTime
                : mouseSensitivity.y * 0.1f;

            if (lookDelta.sqrMagnitude >= 0.01f)
            {
                _cinemachineTargetYaw += lookDelta.x * sensitivityX;
                _cinemachineTargetPitch += lookDelta.y * sensitivityY;
            }

            _cinemachineTargetYaw = ClampAngle(
                _cinemachineTargetYaw,
                float.MinValue,
                float.MaxValue
            );
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

            _cameraTarget.rotation = Quaternion.Euler(
                -_cinemachineTargetPitch,
                _cinemachineTargetYaw,
                0.0f
            );
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f)
                lfAngle += 360f;
            if (lfAngle > 360f)
                lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}
