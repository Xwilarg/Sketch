using UnityEngine;

namespace Sketch.FPS
{
    public class FPSManager : MonoBehaviour
    {
        public static FPSManager Instance { private set; get; }

        public Switch ActiveSwitch { set; get; }

        private void Awake()
        {
            Instance = this;
        }
    }
}