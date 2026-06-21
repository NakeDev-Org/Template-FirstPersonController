using UnityEngine;

namespace nakatimat.TPS.Player.Modular.Data
{
    [CreateAssetMenu(
        fileName = "PlayerCapsuleStats",
        menuName = "NakeD/Player/Capsule Stats"
    )]
    public class PlayerCapsuleStats : ScriptableObject
    {
        [Header("Standing Dimensions")]
        public float StandingHeight = 1.8f;
        public float StandingCenter = 0.9f;

        [Header("Crouching Dimensions")]
        public float CrouchingHeight = 1.2f;
        public float CrouchingCenter = 0.6f;

        [Header("Collision Info")]
        public float Radius = 0.3f;
        public float GroundCheckRadius = 0.1f;
        public float GroundedOffset = -0.1f;
        public LayerMask GroundLayerMask;
        public LayerMask ObstacleMask;
    }
}
