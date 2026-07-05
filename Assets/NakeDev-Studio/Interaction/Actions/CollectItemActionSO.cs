using nakatimat.InteractionSystem;
using nakatimat.InventorySystem;
using UnityEngine;

namespace nakatimat.InventorySystem.Actions
{
    /// <summary>
    /// Ação modular de inventário.
    /// Permite que qualquer InteractableObject funcione como um coletável (ex: Ammo, Keys, Medkits).
    /// </summary>
    [CreateAssetMenu(menuName = "NakeDev/Interaction/Actions/Functions/Collect Item")]
    public class CollectItemActionSO : InteractionActionSO
    {
        [TextArea(2, 3)]
        public string DeveloperNote = "ℹ️ DICA: Usa a interface ICollectible para coletar e enviar o item ao inventário.";

        [Header("Item Data")]
        [Tooltip("O ID único desse item no inventário (ex: 'ammo_pistol', 'key_mansion')")]
        public string itemID = "item_name";
        
        [Tooltip("Quantidade que será adicionada ao inventário ao coletar")]
        public int amount = 1;

        public override void Execute(GameObject interactor, GameObject target)
        {
            // 1. Dispara o evento global para o InventoryManager (e outros sistemas) escutarem
            InventoryEvents.TriggerItemCollected(itemID, amount);

            // 2. Destrói o objeto físico do mundo, já que ele foi coletado
            if (target != null)
            {
                Destroy(target);
            }
        }
    }
}
