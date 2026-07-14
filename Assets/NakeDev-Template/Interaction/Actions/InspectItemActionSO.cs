using UnityEngine;

namespace nakatimat.InteractionSystem.Actions
{
    /// <summary>
    /// Ação modular: ao interagir, o item entra em modo de inspeção (em vez de ir direto pro inventário).
    /// O jogador então decide, olhando o item de perto, se quer coletá-lo ou devolvê-lo ao mundo.
    /// </summary>
    [CreateAssetMenu(menuName = "NakeDev/Interaction/Actions/Functions/Inspect Item")]
    public class InspectItemActionSO : InteractionActionSO
    {
        [TextArea(2, 3)]
        public string DeveloperNote = "ℹ️ DICA: Use esta ação em vez de 'Collect Item' quando quiser que o jogador examine o objeto antes de decidir levá-lo ou não.";

        [Header("Item Data")]
        [Tooltip("O ID único desse item no inventário (ex: 'ammo_pistol', 'key_mansion')")]
        public string itemID = "item_name";

        [Tooltip("Quantidade que será adicionada ao inventário se o jogador coletar o item")]
        public int amount = 1;

        public override void Execute(GameObject interactor, GameObject target)
        {
            nakatimat.InspectSystem.InspectSystem inspectSystem = interactor.GetComponentInParent<nakatimat.InspectSystem.InspectSystem>();

            if (inspectSystem == null)
            {
                Debug.LogWarning($"[InspectItemActionSO] Nenhum InspectSystem encontrado em '{interactor.name}' ou em seus pais.");
                return;
            }

            // Item já foi inspecionado e coletado antes (mesmo que consumível e zerado agora):
            // coleta direto, sem passar pelo modo de inspeção de novo.
            if (inspectSystem.HasBeenInspected(itemID))
            {
                nakatimat.InventorySystem.InventoryEvents.TriggerItemCollected(itemID, amount);
                Destroy(target);
                return;
            }

            inspectSystem.BeginInspect(target, itemID, amount);
        }
    }
}
