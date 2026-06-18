using UnityEngine;

namespace nakatimat.RangedFramework
{
    public enum RangedWeaponType
    {
        Bow, // Atira ao soltar a mira
        Firearm, // Atira ao apertar ataque enquanto mira
    }

    [CreateAssetMenu(
        fileName = "New RangedWeapon",
        menuName = "NakeCore/ArcherFramework/Ranged Weapon Stats"
    )]
    public class RangedWeaponStats : ScriptableObject
    {
        [Header("Weapon Behavior")]
        [Tooltip(
            "Bow: Dispara ao soltar o gatilho de mira.\nFirearm: Dispara ao apertar o botão de ataque durante a mira."
        )]
        public RangedWeaponType WeaponType = RangedWeaponType.Bow;

        [Header("Models & Instantiation")]
        [Tooltip("O Prefab 3D da Arma/Arco que será instanciado na mão/costas do personagem.")]
        public GameObject WeaponPrefab;

        [Tooltip(
            "Opcional: O Prefab 3D da Aljava/Coldre que ficará permanentemente nas costas/cintura."
        )]
        public GameObject QuiverPrefab;

        [Header("Projectile Info")]
        public GameObject ProjectilePrefab;

        [Tooltip("Velocidade do projétil ao sair da arma.")]
        public float ProjectileSpeed = 30f;

        [Tooltip("Dano base infligido no impacto.")]
        public float ProjectileDamage = 15f;

        [Header("Timing")]
        [Tooltip("Tempo necessário de cooldown entre os disparos.")]
        public float TimeBetweenShots = 0.5f;
    }
}
