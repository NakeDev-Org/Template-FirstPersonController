using nakatimat.Core.Interfaces;
using UnityEngine;

namespace nakatimat.InteractionSystem.Actions
{
    [CreateAssetMenu(menuName = "NakeD/Interactions/Block Player Movement Action")]
    public class BlockPlayerMovementActionSO : InteractionActionSO
    {
        [Tooltip("True para TRAVAR o movimento do Player. False para DESTRAVAR.")]
        public bool BlockMovement = true;

        public override void Execute(
            GameObject interactor,
            GameObject interactedObject
        )
        {
            if (interactor != null)
            {
                var blocker = interactor.GetComponent<IMovementBlocker>();
                if (blocker != null)
                {
                    blocker.SetMovmentBlocked(BlockMovement);
                }
                else
                {
                    Debug.LogWarning($"[InteractionSystem] O interactor {interactor.name} não possui um IMovementBlocker.");
                }
            }
        }
    }
}
