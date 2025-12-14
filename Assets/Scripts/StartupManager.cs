using Sketch.Translation;
using UnityEngine;

namespace Sketch
{
    public class StartupManager : MonoBehaviour
    {
        private void Awake()
        {
            Translate.Instance.SetLanguages(new string[]
            {
                "english", "french"//, "japanese"
            });
        }
    }
}