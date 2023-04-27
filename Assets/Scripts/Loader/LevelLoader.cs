using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sketch.Loader
{
    public class LevelLoader : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string s)
        {
            SceneManager.LoadScene(s);
        }
    }
}
