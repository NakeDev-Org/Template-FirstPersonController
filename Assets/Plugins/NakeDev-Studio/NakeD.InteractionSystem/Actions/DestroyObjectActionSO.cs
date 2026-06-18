using UnityEngine;

namespace nakatimat.InteractionSystem.Actions
{
    [CreateAssetMenu(menuName = "NakeD/Interactions/Destroy Object Action")]
    public class DestroyObjectActionSO : InteractionActionSO
    {
        public override void Execute(GameObject interactor, GameObject interactedObject)
        {
            // Destrói o objeto que sofreu a interação (ex: pegando uma moeda, quebrando um vaso)
            if (interactedObject != null)
            {
                Destroy(interactedObject);
            }
        }
    }
}
