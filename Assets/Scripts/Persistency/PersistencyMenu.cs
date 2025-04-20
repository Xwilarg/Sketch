using Sketch.Translation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sketch.Persistency
{
    public class PersistencyMenu : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _debugPersistency;

        private void Awake()
        {
            _debugPersistency.text = Translate.Instance.Tr("persistency", PersistencyManager.Instance.PersistencySize.ToString());
        }

        public void DeleteSave()
        {
            PersistencyManager.Instance.DeleteSave();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}