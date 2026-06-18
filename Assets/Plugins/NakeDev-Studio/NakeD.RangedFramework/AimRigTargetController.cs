using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace nakatimat.RangedFramework
{
    [RequireComponent(typeof(CharacterAimController))]
    public class AimRigTargetController : MonoBehaviour
    {
        [Header("Rigging References")]
        [Tooltip(
            "Arraste apenas o seu Aim_Spine_Constraint aqui. O script vai ligar/desligar apenas o peso dele, sem afetar o resto do Rig!"
        )]
        [SerializeField]
        private MultiAimConstraint _spineAimConstraint;

        [Tooltip(
            "O GameObject alvo que o Multi-Aim vai seguir (aquele que criamos solto na câmera)."
        )]
        [SerializeField]
        private Transform _aimTarget;

        [Header("Settings")]
        [Tooltip("Velocidade para ligar/desligar a mira.")]
        [SerializeField]
        private float _rigBlendSpeed = 10f;

        [Tooltip("Distância que o alvo virtual vai ficar na frente da câmera.")]
        [SerializeField]
        private float _targetDistance = 50f;

        private CharacterAimController _aimController;
        private Camera _mainCamera;
        private float _targetWeight;

        private void Awake()
        {
            _aimController = GetComponent<CharacterAimController>();
            _mainCamera = Camera.main;

            if (_spineAimConstraint != null)
            {
                _spineAimConstraint.weight = 0f;
            }
        }

        private void Update()
        {
            if (_spineAimConstraint == null || _aimTarget == null || _mainCamera == null)
                return;

            // Define se o Constraint deve estar ligado ou desligado
            _targetWeight = _aimController.IsAiming ? 1f : 0f;

            // Suaviza a transição do peso apenas deste Constraint
            _spineAimConstraint.weight = Mathf.Lerp(
                _spineAimConstraint.weight,
                _targetWeight,
                Time.deltaTime * _rigBlendSpeed
            );

            // Se estiver mirando (ou quase), movemos o Target
            if (_spineAimConstraint.weight > 0.01f)
            {
                // Faz o Target flutuar muito longe, exatamente na direção central da câmera
                _aimTarget.position =
                    _mainCamera.transform.position
                    + (_mainCamera.transform.forward * _targetDistance);
            }
        }
    }
}
