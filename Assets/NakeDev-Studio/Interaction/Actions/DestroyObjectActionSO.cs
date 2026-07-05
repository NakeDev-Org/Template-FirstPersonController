using UnityEngine;

namespace nakatimat.InteractionSystem.Actions
{
    [CreateAssetMenu(menuName = "NakeDev/Interaction/Actions/Functions/Destroy Object")]
    public class DestroyObjectActionSO : InteractionActionSO
    {
        [TextArea(2, 3)]
        public string DeveloperNote = "ℹ️ DICA: Use esta ação para destruir o próprio item clicado (ou o alvo).";

        public override void Execute(
            GameObject interactor,
            GameObject interactedObject
        )
        {
            // Destrói o objeto que sofreu a interação (ex: pegando uma moeda, quebrando um vaso)
            if (interactedObject != null)
            {
                Destroy(interactedObject);
            }
        }
    }
}
