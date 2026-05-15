using UnityEngine;

namespace Sketch.FPS
{
    [CreateAssetMenu(menuName = "ScriptableObject/PlayerControlInfo", fileName = "PlayerControlInfo")]
    public class PlayerControlInfo : ScriptableObject
    {
        public float MouvementSpeed = 5f;

        public float RunningMultiplier = 1.5f;

        public float JumpForce = 2f;

        public float GravityMultiplier = .75f;
    }
}
