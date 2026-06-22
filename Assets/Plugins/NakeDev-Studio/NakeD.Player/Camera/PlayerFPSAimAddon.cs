using System;
using nakatimat.Core.Interfaces;
using UnityEngine;
using nakatimat.Core.Inspector;

namespace nakatimat.TPS.Player.Modular
{
    [RequireComponent(typeof(InputReader))]
    public class PlayerFPSAimAddon : MonoBehaviour, IAimingAddon
    {
        [Separator("FPS Camera", 50, 200, 255)]
        [Tooltip("A Câmera Principal do FPS (ex: Main Camera posicionada na cabeça do Player).")]
        [SerializeField] protected Camera _fpsCamera;

        [Separator("Sensibilidade da Câmera (Mouse/Gamepad)", 180, 255, 100)]
        public Vector2 mouseSensitivity = new Vector2(2f, 2f);
        public Vector2 gamepadSensitivity = new Vector2(150f, 150f);

        [Separator("Restrições de Ângulo (Clamp)", 255, 100, 100)]
        public float bottomClamp = -89f;
        public float topClamp = 89f;

        [Separator("Suavização/Aceleração", 100, 255, 200)]
        public bool enableSmoothing = true;
        public float cameraAcceleration = 5f;

        [Separator("UI", 255, 100, 200)]
        [Tooltip("Arraste o GameObject/UI da sua retícula de mira (Crosshair) aqui.")]
        [SerializeField] protected GameObject _crosshairUI;

        // IAimingAddon implementation
        public bool IsAiming { get; protected set; }
        public event Action<bool> OnAimStateChanged;

        protected InputReader _inputReader;
        protected ICombatAddon _combatAddon;
        
        // Internal State
        protected float _cameraPitch = 0f;
        protected float _playerYaw = 0f;

        protected virtual void Awake()
        {
            _playerYaw = transform.eulerAngles.y;
            _inputReader = GetComponent<InputReader>();
            _combatAddon = GetComponent<ICombatAddon>();
            
            if (_fpsCamera == null)
            {
                _fpsCamera = GetComponentInChildren<Camera>();
            }

            if (_crosshairUI != null)
                _crosshairUI.SetActive(true); // Crosshair sempre ativa em FPS padrão
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
            if (_combatAddon != null)
            {
                if (_combatAddon.IsMeleeStance) return;
                if (!_combatAddon.HasRangedWeapon) return;
            }

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

            if (lookDelta.sqrMagnitude >= 0.01f || enableSmoothing)
            {
                float yawDelta = lookDelta.x * sensitivityX;
                float pitchDelta = lookDelta.y * sensitivityY;

                _playerYaw += yawDelta;
                _cameraPitch -= pitchDelta;
                _cameraPitch = Mathf.Clamp(_cameraPitch, bottomClamp, topClamp);

                Quaternion targetBodyRotation = Quaternion.Euler(0f, _playerYaw, 0f);
                Quaternion targetCameraRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);

                if (enableSmoothing)
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
