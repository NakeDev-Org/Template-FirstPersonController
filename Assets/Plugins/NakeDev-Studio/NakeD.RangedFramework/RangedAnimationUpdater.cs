using nakatimat.TPS.Player;
using UnityEngine;

namespace nakatimat.RangedFramework
{
    [RequireComponent(typeof(CharacterAimController))]
    public class RangedAnimationUpdater : MonoBehaviour
    {
        private CharacterAimController _aimController;
        private CharacterAnimationHandler _animationHandler;

        private void Awake()
        {
            _aimController = GetComponent<CharacterAimController>();

            // Tenta pegar o CharacterAnimationHandler no mesmo objeto ou nos filhos
            _animationHandler = GetComponent<CharacterAnimationHandler>();
            if (_animationHandler == null)
            {
                _animationHandler = GetComponentInChildren<CharacterAnimationHandler>();
            }
        }

        private void Update()
        {
            if (_aimController == null || _animationHandler == null)
                return;

            // Sincroniza o estado da mira com o Animator a cada frame
            _animationHandler.UpdateAiming(_aimController.IsAiming);
        }
    }
}
