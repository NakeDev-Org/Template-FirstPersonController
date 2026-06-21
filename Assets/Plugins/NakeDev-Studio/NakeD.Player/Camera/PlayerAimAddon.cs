using System;
using nakatimat.Core.Interfaces;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace nakatimat.TPS.Player.Modular
{
    [RequireComponent(typeof(InputReader))]
    public class PlayerAimAddon : MonoBehaviour, IAimingAddon
    {
        [Header("Camera Targets")]
        [Tooltip("O objeto vazio (ex: CameraTarget) que o Cinemachine deve seguir.")]
        [SerializeField] protected Transform _cameraTarget;

        [Header("Cinemachine Cameras")]
        [Tooltip("Arraste a câmera de mira (CM vcam_Aiming) aqui.")]
        [SerializeField] protected CinemachineCamera _aimCamera;
        [Tooltip("A prioridade que a câmera terá quando estiver mirando.")]
        [SerializeField] protected int _activePriority = 20;
        [Tooltip("A prioridade que a câmera terá quando NÃO estiver mirando.")]
        [SerializeField] protected int _inactivePriority = 0;

        [Header("UI")]
        [Tooltip("Arraste o GameObject/UI da sua retícula de mira (Crosshair) aqui.")]
        [SerializeField] protected GameObject _crosshairUI;

        [Header("Rigging (IK)")]
        [Tooltip("Arraste apenas o seu Aim_Spine_Constraint aqui.")]
        [SerializeField] protected MultiAimConstraint _spineAimConstraint;
        [Tooltip("O GameObject alvo que o Multi-Aim vai seguir (o flutuante).")]
        [SerializeField] protected Transform _aimTarget;
        [Tooltip("Velocidade para ligar/desligar a mira no Rig.")]
        [SerializeField] protected float _rigBlendSpeed = 10f;
        [Tooltip("Distância que o alvo virtual vai ficar na frente da câmera.")]
        [SerializeField] protected float _targetDistance = 50f;

        [Header("Sensibilidade da Câmera (Mouse/Gamepad)")]
        public Vector2 mouseSensitivity = new Vector2(2f, 2f);
        public Vector2 gamepadSensitivity = new Vector2(150f, 150f);

        [Header("Restrições de Ângulo (Clamp)")]
        public float bottomClamp = -60f;
        public float topClamp = 60f;

        [Header("Suavização (Smoothing)")]
        public bool enableSmoothing = true;
        [Range(0.01f, 0.5f)] public float smoothTime = 0.05f;

        // IAimingAddon implementation
        public bool IsAiming { get; protected set; }

        // Events
        public event Action<bool> OnAimStateChanged;

        // Internal State
        protected InputReader _inputReader;
        protected ICombatAddon _combatAddon;
        protected Camera _mainCamera;

        protected float _cinemachineTargetPitch;
        protected float _cinemachineTargetYaw;
        protected Vector2 _currentLookInput;
        protected Vector2 _lookInputVelocity;
        protected float _targetRigWeight;

        protected virtual void Awake()
        {
            _inputReader = GetComponent<InputReader>();
            _combatAddon = GetComponent<ICombatAddon>(); // Pode ser nulo se for jogo sem combate
            _mainCamera = Camera.main;

            if (_crosshairUI != null)
                _crosshairUI.SetActive(false);

            if (_spineAimConstraint != null)
                _spineAimConstraint.weight = 0f;
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
            if (_cameraTarget != null)
            {
                _cinemachineTargetYaw = _cameraTarget.rotation.eulerAngles.y;
            }
            else
            {
                Debug.LogWarning("PlayerAimAddon: _cameraTarget não assinalado! A câmera não vai rotacionar.", this);
            }
        }

        protected virtual void Update()
        {
            UpdateRigging();
            UpdateCameraState();
        }

        protected virtual void LateUpdate()
        {
            CameraRotation();
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

        protected virtual void CameraRotation()
        {
            if (_inputReader == null || _cameraTarget == null)
                return;

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

            float sensitivityX = isGamepad ? gamepadSensitivity.x * Time.deltaTime : mouseSensitivity.x * 0.1f;
            float sensitivityY = isGamepad ? gamepadSensitivity.y * Time.deltaTime : mouseSensitivity.y * 0.1f;

            if (lookDelta.sqrMagnitude >= 0.01f)
            {
                _cinemachineTargetYaw += lookDelta.x * sensitivityX;
                _cinemachineTargetPitch += lookDelta.y * sensitivityY;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

            _cameraTarget.rotation = Quaternion.Euler(-_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
        }

        protected virtual void UpdateCameraState()
        {
            if (_aimCamera == null)
                return;

            if (IsAiming)
            {
                if (!_aimCamera.gameObject.activeSelf)
                {
                    _aimCamera.gameObject.SetActive(true);
                    _aimCamera.Priority = _activePriority;
                    if (_crosshairUI != null) _crosshairUI.SetActive(true);
                }
            }
            else
            {
                if (_aimCamera.gameObject.activeSelf)
                {
                    _aimCamera.Priority = _inactivePriority;
                    _aimCamera.gameObject.SetActive(false);
                    if (_crosshairUI != null) _crosshairUI.SetActive(false);
                }
            }
        }

        protected virtual void UpdateRigging()
        {
            if (_spineAimConstraint == null || _aimTarget == null || _mainCamera == null)
                return;

            _targetRigWeight = IsAiming ? 1f : 0f;

            _spineAimConstraint.weight = Mathf.Lerp(
                _spineAimConstraint.weight,
                _targetRigWeight,
                Time.deltaTime * _rigBlendSpeed
            );

            if (_spineAimConstraint.weight > 0.01f)
            {
                _aimTarget.position = _mainCamera.transform.position + (_mainCamera.transform.forward * _targetDistance);
            }
        }

        protected static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}
