using UnityEngine;

namespace nakatimat.RangedFramework
{
    /// <summary>
    /// Coloque este script no Prefab do seu Arco ou Arma de Fogo.
    /// Isso permite que o sistema saiba exatamente de onde o tiro deve sair.
    /// </summary>
    public class RangedWeaponInstance : MonoBehaviour
    {
        [Tooltip("O ponto (Transform vazio) na ponta da arma/arco de onde o projétil vai nascer.")]
        public Transform MuzzlePoint;
    }
}
