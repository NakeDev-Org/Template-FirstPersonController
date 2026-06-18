using UnityEngine;

namespace nakatimat.TPS.Player.Modular.Data
{
    [CreateAssetMenu(
        fileName = "PlayerLocomotionStats",
        menuName = "NakeCore/Player/Locomotion Stats"
    )]
    public class PlayerLocomotionStats : ScriptableObject
    {
        [Header("Movement Speeds")]
        public float WalkSpeed = 3f;
        public float CrouchSpeed = 2f;
        public float SprintSpeed = 6f;

        [Header("Melee Movement Speeds")]
        public float MeleeWalkSpeed = 2f;
        public float MeleeSprintSpeed = 4f;

        [Header("Responsiveness")]
        public float RotationSmoothing = 10f;
        public float SpeedChangeDamping = 10f;

        [Header("Jumping & Gravity")]
        [Tooltip(
            "Desmarque para impedir que o personagem pule (Ex: dentro de casas ou em jogos sem pulo)."
        )]
        public bool CanJump = true;
        public float JumpForce = 5f;
        public float GravityMultiplier = 2f;
        public float TerminalVelocity = -53f;

        [Header("Stamina Costs")]
        public bool RequireStaminaToSprint = true;
        public float SprintStaminaCostPerSecond = 15f;

        [Space]
        public bool RequireStaminaToJump = true;
        public float JumpStaminaCost = 10f;
    }
}
