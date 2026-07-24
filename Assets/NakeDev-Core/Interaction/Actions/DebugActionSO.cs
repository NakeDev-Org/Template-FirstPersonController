using UnityEngine;

namespace nakatimat.InteractionSystem.Actions
{
    [CreateAssetMenu(menuName = "NakeDev/Interaction/Actions/Debug/Log Message")]
    public class DebugActionSO : InteractionActionSO
    {
        [TextArea(2, 3)]
        public string DeveloperNote = "ℹ️ DICA: Apenas imprime uma mensagem no Console para testes de lógica.";

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
