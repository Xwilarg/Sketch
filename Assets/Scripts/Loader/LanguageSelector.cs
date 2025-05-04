using Sketch.Translation;
using UnityEngine;

namespace Sketch.Loader
{
    public class LanguageSelector : MonoBehaviour
    {
        public void ChangeLanguage()
        {
            if (Translate.Instance.CurrentLanguage == "english") Translate.Instance.CurrentLanguage = "french";
            else if (Translate.Instance.CurrentLanguage == "french") Translate.Instance.CurrentLanguage = "japanese";
            else Translate.Instance.CurrentLanguage = "english";
        }
    }
}
