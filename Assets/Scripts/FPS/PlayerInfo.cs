using UnityEngine;

namespace Sketch.FPS
{
    [CreateAssetMenu(menuName = "ScriptableObject/PlayerInfo", fileName = "PlayerInfo")]
    public class PlayerInfo : ScriptableObject
    {
        [Header("Configuration")]

        [Tooltip("Movement speed of the player")]
        public float MovementSpeed;

        [Range(0, 10f)]
        [Tooltip("Sensitivity of mouse on X axis")]
        public float HorizontalLookMultiplier = .1f;

        [Range(0, 10f)]
        [Tooltip("Sensitivity of mouse on Y axis")]
        public float VerticalLookMultiplier = .1f;

        [Range(0f, 10f)]
        [Tooltip("Speed of the player")]
        public float ForceMultiplier = 1f;

        [Range(0f, 10f)]
        [Tooltip("Speed multiplicator when the player is running")]
        public float SpeedRunningMultiplicator;

        [Range(0f, 10f)]
        [Tooltip("Vertical force used to make the player jump")]
        public float JumpForce;

        [Tooltip("Gravity multiplier to make the player fall")]
        public float GravityMultiplicator;
    }
}