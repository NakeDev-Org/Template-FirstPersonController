using UnityEngine;

namespace nakatimat.InteractionSystem.Actions
{
    [CreateAssetMenu(menuName = "NakeDev/Interaction/Actions/Functions/Play Animation")]
    public class PlayAnimationActionSO : InteractionActionSO
    {
        [TextArea(2, 3)]
        public string DeveloperNote = "ℹ️ DICA: Toca uma animação no Animator (ex: abrir porta, puxar alavanca).";

        [Tooltip("O nome do estado de animação a ser tocado no Animator (ex: 'Pickup_Ground')")]
        public string AnimationStateName = "Pickup_Ground";

        [Tooltip("Tempo de suavização (Crossfade) para entrar na animação.")]
        public float CrossfadeDuration = 0.1f;

        public override void Execute(
            GameObject interactor,
            GameObject interactedObject
        )
        {
            if (interactor != null)
            {
                var animator = interactor.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.CrossFade(AnimationStateName, CrossfadeDuration);
                }
            }
        }
    }
}
