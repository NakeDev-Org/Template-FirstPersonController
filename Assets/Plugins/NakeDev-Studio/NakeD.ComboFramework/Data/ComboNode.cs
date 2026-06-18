using System.Collections.Generic;
using UnityEngine;

namespace nakatimat.ComboFramework.Data
{
    [System.Serializable]
    public struct ComboBranch
    {
        [Tooltip("Que botão o jogador tem que apertar para esse ataque sair?")]
        public AttackInputType requiredInput;

        [Tooltip("Qual o próximo golpe (Node) que será tocado?")]
        public ComboNode nextNode;
    }

    [CreateAssetMenu(
        fileName = "New Combo Node",
        menuName = "NakeCore/Combo Framework/Combo Node"
    )]
    public class ComboNode : ScriptableObject
    {
        [Header("Animation")]
        [Tooltip("O nome EXATO do estado da animação lá no Animator Base.")]
        public string animationStateName = "Attack_01";

        [Header("Combat Stats")]
        [Tooltip("Multiplicador aplicado sobre o Dano Base da Arma equipada.")]
        public float damageMultiplier = 1.0f;

        [Tooltip(
            "Custo individual de stamina para executar apenas este ataque."
        )]
        public float staminaCost = 15f;

        [Header("Conditions")]
        public bool requireGrounded = true;

        [Tooltip(
            "Direção exigida no direcional (InputReader) para executar este golpe. Deixe Vector2.zero para ataques normais/neutros."
        )]
        public Vector2 requiredMoveDirection = Vector2.zero;

        [Header("Normalized Timings (0.0 to 1.0)")]
        [Range(0f, 1f), Tooltip("Porcentagem em que a Hitbox (Dano) LIGA.")]
        public float hitboxStartTime = 0.3f;

        [Range(0f, 1f), Tooltip("Porcentagem em que a Hitbox (Dano) DESLIGA.")]
        public float hitboxEndTime = 0.5f;

        [Space(10)]
        [
            Range(0f, 1f),
            Tooltip(
                "Porcentagem onde o jogador PODE apertar o botão para emendar o próximo ataque."
            )
        ]
        public float comboWindowStartTime = 0.5f;

        [Range(0f, 1f), Tooltip("Porcentagem onde a janela de combo se FECHA.")]
        public float comboWindowEndTime = 0.8f;

        [Header("Branching (Ramificação)")]
        [Tooltip("Quais ataques podem ser emendados logo APÓS este nó?")]
        public List<ComboBranch> nextPossibleNodes = new List<ComboBranch>();

        [Header("Editor & Preview (Ferramentas)")]
        [Tooltip(
            "Usado apenas pelo AttackPreviewHelper para testar a Hitbox na Scene."
        )]
        public AnimationClip previewClip;

        [Header("VFX Settings (Efeitos)")]
        public GameObject vfxPrefab;
        public Vector3 vfxPosition;
        public Vector3 vfxRotation;
        public float vfxScale = 1f;
        public Color vfxColor = Color.white;

        [Header("HitBox Settings (Física)")]
        public Vector3 hitBoxCenter;
        public Vector3 hitBoxSize;
    }
}
