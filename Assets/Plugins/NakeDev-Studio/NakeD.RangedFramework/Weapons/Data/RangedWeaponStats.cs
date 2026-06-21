using UnityEngine;

namespace nakatimat.RangedFramework
{
    [CreateAssetMenu(
        fileName = "New RangedWeapon",
        menuName = "NakeD/Combat/Ranged Weapon Stats"
    )]
    public class RangedWeaponStats : ScriptableObject
    {
        [Header("Models & Instantiation")]
        [Tooltip(
            "O Prefab 3D da Arma que será instanciado na mão/costas do personagem."
        )]
        public GameObject WeaponPrefab;

        [Tooltip("O Animator Controller específico dessa arma (Substitui as animações base).")]
        public RuntimeAnimatorController animatorOverride;

        [Header("Gun Stats (Hitscan)")]
        [Tooltip("Dano base infligido no impacto do hitscan.")]
        public float BaseDamage = 15f;

        [Tooltip("Tempo necessário de cooldown entre os disparos (Fire Rate).")]
        public float TimeBetweenShots = 0.5f;

        [Tooltip("Layers que o hitscan da arma pode acertar.")]
        public LayerMask HitMask = Physics.DefaultRaycastLayers;

        [Header("Ammo System")]
        [Tooltip("Tamanho do pente (Clip)")]
        public int ClipSize = 10;

        [Tooltip("Tempo em segundos para recarregar a arma.")]
        public float ReloadTime = 1.5f;
    }
}
