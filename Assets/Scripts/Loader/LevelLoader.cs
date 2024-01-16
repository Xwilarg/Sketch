using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sketch.Loader
{
    public class LevelLoader : MonoBehaviour
    {
        public static LevelLoader Instance { private set; get; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance.GetInstanceID() != GetInstanceID())
            {
                Destroy(gameObject);
                return;
            }

            SceneManager.activeSceneChanged += (sender, e) =>
            {
                SceneManager.LoadScene("AchievementManager", LoadSceneMode.Additive);
            };

            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string s)
        {
            SceneManager.LoadScene(s);
        }
    }
}
