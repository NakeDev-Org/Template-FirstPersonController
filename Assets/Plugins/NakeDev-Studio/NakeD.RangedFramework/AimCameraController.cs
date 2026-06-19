using nakatimat.Core.Interfaces;
using Unity.Cinemachine;
using UnityEngine;

namespace nakatimat.RangedFramework
{
    public class AimCameraController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Arraste a câmera de mira (CM vcam_Aim) aqui.")]
        [SerializeField]
        private CinemachineCamera _aimCamera;

        [Tooltip("Arraste o GameObject/UI da sua retícula de mira (Crosshair) aqui.")]
        [SerializeField]
        private GameObject _crosshairUI;

        private IAimingAddon _aimingAddon;

        [Header("Settings")]
        [Tooltip("A prioridade que a câmera terá quando estiver mirando.")]
        [SerializeField]
        private int _activePriority = 20;

        [Tooltip("A prioridade que a câmera terá quando NÃO estiver mirando.")]
        [SerializeField]
        private int _inactivePriority = 0;

        private void Awake()
        {
            _aimingAddon = GetComponent<IAimingAddon>();

            if (_crosshairUI != null)
                _crosshairUI.SetActive(false); // Garante que começa desligado
        }

        private void Update()
        {
            if (_aimCamera == null || _aimingAddon == null)
                return;

            if (_aimingAddon.IsAiming)
            {
                if (!_aimCamera.gameObject.activeSelf)
                {
                    _aimCamera.gameObject.SetActive(true);
                    _aimCamera.Priority = _activePriority;

                    if (_crosshairUI != null)
                        _crosshairUI.SetActive(true);
                }
            }
            else
            {
                if (_aimCamera.gameObject.activeSelf)
                {
                    _aimCamera.Priority = _inactivePriority;
                    _aimCamera.gameObject.SetActive(false);

                    if (_crosshairUI != null)
                        _crosshairUI.SetActive(false);
                }
            }
        }
    }
}
