using UnityEngine;

namespace nakatimat.InteractionSystem
{
    /// <summary>
    /// Base class for all interaction actions.
    /// Inherit from this to create specific actions like PlayAudioAction, OpenDoorAction, GiveItemAction, etc.
    /// </summary>
    public abstract class InteractionActionSO : ScriptableObject
    {
        /// <summary>
        /// Executes the specific action.
        /// </summary>
        /// <param name="interactor">The player or entity triggering the interaction.</param>
        /// <param name="interactedObject">The object being interacted with.</param>
        public abstract void Execute(GameObject interactor, GameObject interactedObject);
    }
}
