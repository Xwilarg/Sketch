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
            _debugPersistency.text = Translate.Instance.Tr("persistency", PersistencyManager<SaveData>.Instance.PersistencySize.ToString());
        }

        public void DeleteSave()
        {
            PersistencyManager<SaveData>.Instance.DeleteSave();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}