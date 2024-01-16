using Sketch.Persistency;
using Sketch.Translation;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Sketch.Achievement
{
    public class AchievementManager : MonoBehaviour
    {
        [SerializeField]
        private Transform _container;

        [SerializeField]
        private GameObject _prefab;

        public static AchievementManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void Unlock(AchievementID achievement)
        {
            if (PersistencyManager.Instance.SaveData.IsUnlocked(achievement))
            {
                return;
            }
            var instance = Instantiate(_prefab, _container);

            var data = Achievements[achievement];
            instance.GetComponentInChildren<TMP_Text>().text = data.Name;

            PersistencyManager.Instance.SaveData.Unlock(achievement);
            PersistencyManager.Instance.Save();

            Destroy(instance, 2f);
        }

        public Dictionary<AchievementID, Achievement> Achievements { get; } = new()
        {
            { AchievementID.GEN_noDoor, new("landlocked") },
            { AchievementID.FIS_150cm, new("150cm") },
            { AchievementID.VIS_NoSkip, new("noSkip") },
        };
    }

    public enum AchievementID
    {
        GEN_noDoor,
        FIS_150cm,
        VIS_NoSkip
    }

    public record Achievement
    {
        public Achievement(string id)
        {
            _id = id;
        }

        private string _id;
        public string Name => Translate.Instance.Tr($"ACH_{_id}Name");
        public string Description => Translate.Instance.Tr($"ACH_{_id}Desc");
        public string Hint => Translate.Instance.Tr($"ACH_{_id}Hint");
    }
}