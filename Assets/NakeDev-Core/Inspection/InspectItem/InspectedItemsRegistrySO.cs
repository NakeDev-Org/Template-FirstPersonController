using System.Collections.Generic;
using UnityEngine;

namespace nakatimat.InspectSystem
{
    /// <summary>
    /// Registro global (ScriptableObject) dos itemIDs que o jogador já inspecionou e coletou
    /// pelo menos uma vez. Independente da quantidade atual no inventário: mesmo que um item
    /// consumível acabe, ele não precisa passar pela inspeção de novo.
    /// </summary>
    [CreateAssetMenu(fileName = "InspectedItemsRegistry", menuName = "NakeDev/Inspection/Inspected Items Registry")]
    public class InspectedItemsRegistrySO : ScriptableObject
    {
        private readonly HashSet<string> _inspectedItemIDs = new HashSet<string>();

        private void OnEnable()
        {
            // Zera a cada sessão de Play (o asset é compartilhado e sobreviveria entre execuções no Editor).
            _inspectedItemIDs.Clear();
        }

        public bool HasBeenInspected(string itemID) => _inspectedItemIDs.Contains(itemID);

        public void MarkInspected(string itemID)
        {
            if (!string.IsNullOrEmpty(itemID))
                _inspectedItemIDs.Add(itemID);
        }
    }
}
