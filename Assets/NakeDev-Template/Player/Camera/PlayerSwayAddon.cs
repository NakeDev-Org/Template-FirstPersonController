using System.Collections.Generic;
using UnityEngine;
using nakatimat.Core.Inspector;

namespace nakatimat.Player
{
    /// <summary>
    /// Adiciona um efeito de Sway (arrasto/inclinação) que responde ao movimento do mouse.
    /// Script unificado: Pode ser configurado para atuar na Câmera (Pitch/Roll) ou na Arma (X/Y).
    /// </summary>
    public class PlayerSwayAddon : MonoBehaviour
    {
        public enum SwayTargetType
        {
            Camera,
            WeaponOrArms
        }

        [Header("Sway Setup")]
        [Tooltip("Escolha se este script vai balançar a Câmera (eixo Z) ou os Braços/Arma (eixo X e Y).")]
        public SwayTargetType swayType = SwayTargetType.WeaponOrArms;

        [InspectorLine("Camera Settings", 50, 200, 255)]
        public Camera cameraTarget;
        [Tooltip("Multiplicador da inclinação. Valores positivos fazem a câmera inclinar contra o movimento.")]
        public float cameraSwayMultiplier = 1.5f;
        public float cameraSmooth = 10f;
        public float cameraMaxTiltAngle = 3f;

        [InspectorLine("Weapon/Arms Settings", 255, 100, 100)]
        [Tooltip("Lista de ossos/transforms que receberão o sway.")]
        public List<Transform> weaponBones = new List<Transform>();
        [Tooltip("Multiplicador da força do arrasto. Valores negativos invertem a direção.")]
        public float weaponSwayMultiplier = 2f;
        public float weaponSmooth = 8f;
        public float weaponMaxSwayAngle = 10f;
        [Tooltip("Marque isso se você usar em ossos animados. O sway será somado em cima da animação!")]
        public bool isAnimatedBone = false;

        private InputReader _inputReader;
        
        // Cache for Weapon Sway
        private Quaternion[] _initialLocalRotations;
        private Quaternion _currentWeaponSwayOffset = Quaternion.identity;

        private void Start()
        {
            _inputReader = GetComponentInParent<InputReader>();

            if (swayType == SwayTargetType.Camera)
            {
                if (cameraTarget == null) cameraTarget = GetComponentInChildren<Camera>();
            }
            else
            {
                if (weaponBones.Count == 0)
                {
                    weaponBones.Add(transform);
                }

                _initialLocalRotations = new Quaternion[weaponBones.Count];
                for (int i = 0; i < weaponBones.Count; i++)
                {
                    if (weaponBones[i] != null)
                    {
                        _initialLocalRotations[i] = weaponBones[i].localRotation;
                    }
                }
            }
        }

        private void Update()
        {
            if (_inputReader == null) return;
            
            if (swayType == SwayTargetType.WeaponOrArms)
            {
                ProcessWeaponSway();
            }
        }

        private void LateUpdate()
        {
            if (_inputReader == null) return;

            if (swayType == SwayTargetType.Camera)
            {
                ProcessCameraSway();
            }
            else if (swayType == SwayTargetType.WeaponOrArms && isAnimatedBone)
            {
                ApplyAnimatedBoneOffset();
            }
        }

        private void ProcessCameraSway()
        {
            if (cameraTarget == null) return;

            float mouseX = _inputReader.LookInput.x;
            float targetTilt = Mathf.Clamp(-mouseX * cameraSwayMultiplier, -cameraMaxTiltAngle, cameraMaxTiltAngle);

            Vector3 currentEuler = cameraTarget.transform.localEulerAngles;
            float currentZ = currentEuler.z > 180f ? currentEuler.z - 360f : currentEuler.z;

            float newZ = Mathf.Lerp(currentZ, targetTilt, cameraSmooth * Time.deltaTime);
            cameraTarget.transform.localEulerAngles = new Vector3(currentEuler.x, currentEuler.y, newZ);
        }

        private void ProcessWeaponSway()
        {
            Vector2 lookInput = _inputReader.LookInput;

            float mouseX = Mathf.Clamp(lookInput.x * weaponSwayMultiplier, -weaponMaxSwayAngle, weaponMaxSwayAngle);
            float mouseY = Mathf.Clamp(lookInput.y * weaponSwayMultiplier, -weaponMaxSwayAngle, weaponMaxSwayAngle);

            Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
            Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
            Quaternion targetSway = rotationX * rotationY;

            if (isAnimatedBone)
            {
                _currentWeaponSwayOffset = Quaternion.Slerp(_currentWeaponSwayOffset, targetSway, weaponSmooth * Time.deltaTime);
            }
            else
            {
                for (int i = 0; i < weaponBones.Count; i++)
                {
                    Transform bone = weaponBones[i];
                    if (bone == null) continue;

                    Quaternion targetRotation = _initialLocalRotations[i] * targetSway;
                    bone.localRotation = Quaternion.Slerp(bone.localRotation, targetRotation, weaponSmooth * Time.deltaTime);
                }
            }
        }

        private void ApplyAnimatedBoneOffset()
        {
            for (int i = 0; i < weaponBones.Count; i++)
            {
                Transform bone = weaponBones[i];
                if (bone == null) continue;

                bone.localRotation = bone.localRotation * _currentWeaponSwayOffset;
            }
        }
    }
}
