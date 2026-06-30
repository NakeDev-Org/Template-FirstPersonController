using UnityEngine;

namespace nakatimat.CombatSystem.MeleeSystem
{
    [CreateAssetMenu(
        fileName = "New MeleeWeapon",
        menuName = "NakeD/Combat/Melee Weapon Data"
    )]
    public class MeleeWeaponData : ScriptableObject
    {
        [Header("Models & Instantiation")]
        [Tooltip("O Prefab 3D da Arma que será instanciado.")]
        public GameObject WeaponPrefab;

        [Header("Combat Stats")]
        [Tooltip("Dano bruto da arma.")]
        public float BaseDamage = 20f;
        
        [Tooltip("Pode defender ataques? (Reduz dano frontal)")]
        public bool CanBlock = true;
    }
}
