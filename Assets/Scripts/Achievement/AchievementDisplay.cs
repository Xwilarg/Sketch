using Sketch.Persistency;
using TMPro;
using UnityEngine;

namespace Sketch.Achievement
{
    public class AchievementDisplay : MonoBehaviour
    {
        [SerializeField]
        private Transform _container;

        [SerializeField]
        private GameObject _prefab;

        private void Start()
        {
            foreach (var ach in AchievementManager.Instance.Achievements)
            {
                var unlocked = PersistencyManager.Instance.SaveData.IsUnlocked(ach.Key);

                var obj = Instantiate(_prefab, _container);

                var texts = obj.GetComponentsInChildren<TMP_Text>();

                texts[0].text = unlocked ? ach.Value.Name : "???";
                texts[1].text = unlocked ? ach.Value.Description : ach.Value.Hint;
            }
        }
    }
}
