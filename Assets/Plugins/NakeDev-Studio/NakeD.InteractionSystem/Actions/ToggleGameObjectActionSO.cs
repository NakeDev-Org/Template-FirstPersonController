using UnityEngine;

namespace nakatimat.InteractionSystem.Actions
{
    [CreateAssetMenu(menuName = "NakeD/Interactions/Toggle GameObject Action")]
    public class ToggleGameObjectActionSO : InteractionActionSO
    {
        public enum ToggleType
        {
            Enable,
            Disable,
            ToggleState
        }

        [Tooltip("Como afetar o objeto original (o objeto que foi clicado)?")]
        public ToggleType ActionOnSelf = ToggleType.Disable;

        public override void Execute(
            GameObject interactor,
            GameObject interactedObject
        )
        {
            if (interactedObject != null)
            {
                switch (ActionOnSelf)
                {
                    case ToggleType.Enable:
                        interactedObject.SetActive(true);
                        break;
                    case ToggleType.Disable:
                        interactedObject.SetActive(false);
                        break;
                    case ToggleType.ToggleState:
                        interactedObject.SetActive(!interactedObject.activeSelf);
                        break;
                }
            }
        }
    }
}
