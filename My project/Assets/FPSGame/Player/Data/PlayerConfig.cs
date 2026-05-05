using UnityEngine;

namespace FPSGame.Player
{
    [CreateAssetMenu(menuName = "FPSGame/Player/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float WalkSpeed = 5f;
        public float RunSpeedMultiplier = 1.6f;
        public float CrouchSpeedMultiplier = 0.5f;
        public float JumpForce = 5f;
        public float Gravity = -9.81f;

        [Header("Ground Detection")]
        public float GroundCheckDistance = 0.2f;
        public float SlopeLimit = 45f;
        public LayerMask GroundLayer = 1; // Default layer

        [Header("Timing")]
        public float CoyoteTime = 0.1f;
        public float JumpBuffer = 0.1f;

        [Header("World Bounds")]
        public float FallLimitY = -100f;
    }
}
