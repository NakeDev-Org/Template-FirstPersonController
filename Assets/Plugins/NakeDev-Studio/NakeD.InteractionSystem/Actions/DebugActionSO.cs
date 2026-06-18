using UnityEngine;

namespace nakatimat.InteractionSystem.Actions
{
    [CreateAssetMenu(menuName = "NakeD/Interactions/Debug Action")]
    public class DebugActionSO : InteractionActionSO
    {
        [SerializeField]
        private string _customMessage = "O Player interagiu com este objeto!";

        public override void Execute(
            GameObject interactor,
            GameObject interactedObject
        )
        {
            Debug.Log(
                $"<color=green>[Interaction System]</color> {interactor.name} acionou {_customMessage} no objeto {interactedObject.name}"
            );
        }
    }
}
