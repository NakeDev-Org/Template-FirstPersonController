using UnityEngine;

namespace nakatimat.Player
{
    /// <summary>
    /// Gerencia o Alvo (Target) do Animation Rigging para o modo FPS.
    /// Mantém o alvo sempre no centro da câmera para que o corpo/braços acompanhem a mira.
    /// </summary>
    public class PlayerWeaponRigging : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("A Câmera Principal do FPS")]
        [SerializeField] private Camera _fpsCamera;
        
        [Tooltip("O GameObject vazio que criaremos para ser o alvo do Rig")]
        [SerializeField] private Transform _aimTarget;

        [Header("Settings")]
        [Tooltip("A que distância o alvo será projetado à frente da câmera")]
        [SerializeField] private float _aimDistance = 50f;

        private void Awake()
        {
            if (_fpsCamera == null)
            {
                _fpsCamera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            if (_fpsCamera == null || _aimTarget == null) return;

            // Pega a posição e direção exata do centro da tela
            Ray ray = _fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            // Move o AimTarget para bem longe, exatamente onde a câmera está olhando
            _aimTarget.position = ray.GetPoint(_aimDistance);
        }
    }
}
