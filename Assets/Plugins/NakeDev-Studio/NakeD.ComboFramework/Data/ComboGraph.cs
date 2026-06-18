using UnityEngine;

namespace nakatimat.ComboFramework.Data
{
    [CreateAssetMenu(
        fileName = "New Combo Graph",
        menuName = "NakeCore/Combo Framework/Combo Graph"
    )]
    public class ComboGraph : ScriptableObject
    {
        [Header("Graph Entry")]
        [Tooltip(
            "O nó de ataque inicial da arma (o que toca quando o jogador pressiona o botão a partir da postura neutra)."
        )]
        public ComboNode entryNode;
    }
}
