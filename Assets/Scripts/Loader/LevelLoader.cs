using Sketch.Translation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sketch.Loader
{
    public class LevelLoader : MonoBehaviour
    {
        [SerializeField]
        private GameObject _frenchDisclaimer;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string s)
        {
            SceneManager.LoadScene(s);
        }

        public void ChangeLanguage()
        {
            if (Translate.Instance.CurrentLanguage == "english") Translate.Instance.CurrentLanguage = "french";
            else Translate.Instance.CurrentLanguage = "english";

            _frenchDisclaimer.SetActive(Translate.Instance.CurrentLanguage == "french");
        }
    }
}
