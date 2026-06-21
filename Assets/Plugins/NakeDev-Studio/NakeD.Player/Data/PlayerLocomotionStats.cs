using UnityEngine;

namespace nakatimat.TPS.Player.Modular.Data
{
    public enum MovementWeightProfile
    {
        Standard,   // Com peso/inércia ao arrancar
        Responsive  // Arrancada imediata (Arcade)
    }

    [CreateAssetMenu(
        fileName = "PlayerLocomotionStats",
        menuName = "NakeD/Player/Locomotion Stats"
    )]
    public class PlayerLocomotionStats : ScriptableObject
    {
        [Header("Movement Speeds")]
        public float WalkSpeed = 3f;
        public float SprintSpeed = 6f;

        [Header("Melee Movement Speeds")]
        public float MeleeWalkSpeed = 2f;
        public float MeleeSprintSpeed = 4f;

        [Header("Responsiveness")]
        public MovementWeightProfile WeightProfile = MovementWeightProfile.Standard;
        
        public float RotationSmoothing = 10f;

        [Header("Gravity")]
        public float GravityMultiplier = 2f;
        public float TerminalVelocity = -53f;
    }
}
