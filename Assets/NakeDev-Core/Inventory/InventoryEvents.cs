using System;

namespace nakatimat.InventorySystem
{
    /// <summary>
    /// Eventos globais do sistema de inventário. Desacopla quem emite o evento de quem o ouve.
    /// </summary>
    public static class InventoryEvents
    {
        // O InventoryManager escuta esse evento, e qualquer Ação Modular (como CollectItemActionSO) pode dispará-lo.
        public static event Action<string, int> OnAnyItemCollected;

        public static void TriggerItemCollected(string itemID, int amount)
        {
            OnAnyItemCollected?.Invoke(itemID, amount);
        }
    }
}
