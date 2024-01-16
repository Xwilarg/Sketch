using Sketch.Translation;
using UnityEngine;

namespace Sketch.Loader
{
    public class LanguageSelector : MonoBehaviour
    {
        [SerializeField]
        private GameObject _frenchDisclaimer;

        public void ChangeLanguage()
        {
            if (Translate.Instance.CurrentLanguage == "english") Translate.Instance.CurrentLanguage = "french";
            else Translate.Instance.CurrentLanguage = "english";

            _frenchDisclaimer.SetActive(Translate.Instance.CurrentLanguage == "french");
        }
    }
}
