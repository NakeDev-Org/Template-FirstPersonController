using UnityEngine;

namespace nakatimat.InteractionSystem
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        Sprite InteractionIcon { get; }

        /// <summary>
        /// Called when the player presses the interaction button.
        /// </summary>
        void Interact(GameObject interactor);

        /// <summary>
        /// Gets the exact Vector3 position where the UI floating icon should appear.
        /// </summary>
        Vector3 GetUIPosition();
    }
}
