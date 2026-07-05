using System;
using UnityEngine;
using nakatimat.Core.Inspector;

namespace nakatimat.Player
{
    public enum CameraSmoothingMode
    {
        Disabled,
        Enabled
    }

    [RequireComponent(typeof(InputReader))]
    public class FirstPersonCamera : MonoBehaviour
    {
        [InspectorLine("FPS Camera", 50, 200, 255)]
        [Tooltip("A Câmera Principal do FPS (ex: Main Camera posicionada na cabeça do Player).")]
        [SerializeField] protected Camera _fpsCamera;

        [InspectorLine("Sensibilidade da Câmera (Mouse/Gamepad)", 180, 255, 100)]
        public Vector2 mouseSensitivity = new Vector2(2f, 2f);
        public Vector2 gamepadSensitivity = new Vector2(150f, 150f);

        [InspectorLine("Restrições de Ângulo (Clamp)", 255, 100, 100)]
        public float bottomClamp = -89f;
        public float topClamp = 89f;

        [InspectorLine("Suavização/Aceleração", 100, 255, 200)]
        [Tooltip("Disabled = Rotação instantânea.\nEnabled = Suaviza a rotação da câmera (útil para gamepads).")]
        public CameraSmoothingMode SmoothingMode = CameraSmoothingMode.Enabled;
        public float cameraAcceleration = 15f;



        // Aim State
        public bool IsAiming { get; protected set; }
        public event Action<bool> OnAimStateChanged;

        protected InputReader _inputReader;
        
        // Internal State
        protected float _cameraPitch = 0f;
        protected float _playerYaw = 0f;
        
        // Public Access for Sway scripts
        public float TargetPitch => _cameraPitch;
        public float TargetYaw => _playerYaw;

        protected virtual void Awake()
        {
            _playerYaw = transform.eulerAngles.y;
            _inputReader = GetComponent<InputReader>();
            
            if (_fpsCamera == null)
            {
                _fpsCamera = GetComponentInChildren<Camera>();
            }


        }

        protected virtual void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAimStarted += HandleAimStarted;
                _inputReader.OnAimCanceled += HandleAimCanceled;
            }
        }

        protected virtual void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.OnAimStarted -= HandleAimStarted;
                _inputReader.OnAimCanceled -= HandleAimCanceled;
            }
        }

        protected virtual void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        protected virtual void LateUpdate()
        {
            HandleCameraRotation();
        }

        public virtual void HandleAimStarted()
        {

            IsAiming = true;
            OnAimStateChanged?.Invoke(IsAiming);
        }

        public virtual void HandleAimCanceled()
        {
            IsAiming = false;
            OnAimStateChanged?.Invoke(IsAiming);
        }

        protected virtual void HandleCameraRotation()
        {
            if (_inputReader == null || _fpsCamera == null) return;

            Vector2 lookDelta = _inputReader.LookInput;
            bool isGamepad = _inputReader.IsGamepad;

            float sensitivityX = isGamepad ? gamepadSensitivity.x * Time.deltaTime : mouseSensitivity.x * 0.1f;
            float sensitivityY = isGamepad ? gamepadSensitivity.y * Time.deltaTime : mouseSensitivity.y * 0.1f;

            bool isSmoothingEnabled = SmoothingMode == CameraSmoothingMode.Enabled;

            if (lookDelta.sqrMagnitude >= 0.01f || isSmoothingEnabled)
            {
                float yawDelta = lookDelta.x * sensitivityX;
                float pitchDelta = lookDelta.y * sensitivityY;

                _playerYaw += yawDelta;
                _cameraPitch -= pitchDelta;
                _cameraPitch = Mathf.Clamp(_cameraPitch, bottomClamp, topClamp);

                Quaternion targetBodyRotation = Quaternion.Euler(0f, _playerYaw, 0f);
                Quaternion targetCameraRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);

                if (isSmoothingEnabled)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetBodyRotation, cameraAcceleration * Time.deltaTime);
                    _fpsCamera.transform.localRotation = Quaternion.Lerp(_fpsCamera.transform.localRotation, targetCameraRotation, cameraAcceleration * Time.deltaTime);
                }
                else
                {
                    transform.rotation = targetBodyRotation;
                    _fpsCamera.transform.localRotation = targetCameraRotation;
                }
            }
        }
    }
}

