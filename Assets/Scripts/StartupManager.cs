using Sketch.Translation;
using UnityEngine;

namespace Sketch
{
    public class StartupManager : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;
#endif
            Translate.Instance.SetLanguages(new string[]
            {
                "english", "french"//, "japanese"
            });
        }
    }
}