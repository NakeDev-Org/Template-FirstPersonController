using UnityEngine;

namespace nakatimat.InteractionSystem.Actions
{
    [CreateAssetMenu(menuName = "NakeDev/Interaction/Actions/Functions/Toggle GameObject")]
    public class ToggleGameObjectActionSO : InteractionActionSO
    {
        [TextArea(2, 3)]
        public string DeveloperNote = "ℹ️ DICA: Liga/Desliga o próprio item ou outro GameObject (ex: luz, interruptor).";

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
