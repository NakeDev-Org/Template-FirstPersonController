using UnityEngine;

namespace nakatimat.CombatSystem.MeleeSystem
{
    [CreateAssetMenu(
        fileName = "Defense Data",
        menuName = "NakeD/Combat/Defense Data"
    )]
    public class BlockStats : ScriptableObject
    {
        [Tooltip(
            "Multiplicador de dano aplicado quando o jogador está apenas segurando o bloqueio. 0 = 0% de dano (bloqueio total), 0.5 = 50% de dano."
        )]
        [Range(0f, 1f)]
        public float blockMitigationMultiplier = 0.5f;

        [Tooltip(
            "Janela de tempo em segundos para que o bloqueio seja considerado um Parry."
        )]
        public float parryWindow = 0.25f;

        [Tooltip(
            "Multiplicador de dano aplicado quando o Parry é realizado com sucesso. Se for negativo, irá curar!"
        )]
        [Range(-2f, 1f)]
        public float parryMultiplier = -1f;

        [Tooltip(
            "A velocidade de movimento do personagem é reduzida para este valor durante o bloqueio."
        )]
        public float blockingWalkSpeed = 1f;
    }
}
