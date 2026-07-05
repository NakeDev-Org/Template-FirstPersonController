using UnityEngine;

namespace nakatimat.InteractionSystem
{
    public interface IInteractable
    {
        /// <summary>
        /// Called when the player presses the interaction button.
        /// </summary>
        void Interact(GameObject interactor);

        /// <summary>
        /// Gets the exact Vector3 position where the UI floating icon should appear.
        /// </summary>
        Vector3 GetUIPosition();

        /// <summary>
        /// Chamado pelo InteractionScanner quando o jogador está mirando (true) ou parou de mirar (false) neste item.
        /// </summary>
        void SetTargeted(bool isTargeted);
    }
}
