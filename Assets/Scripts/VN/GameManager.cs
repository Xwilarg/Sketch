using Sketch.Achievement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sketch.VN
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private TextAsset _introStory;

        private bool _didUseSkip;

        private void Start()
        {
            VNManager.Instance.ShowStory(_introStory);

            VNManager.Instance.OnTagParsed.AddListener((string tag, string value) =>
            {
                if (value == "START") _didUseSkip = false;
                else
                {
                    if (!_didUseSkip)
                    {
                        AchievementManager.Instance.Unlock(AchievementID.VIS_NoSkip);
                    }
                }
            });
        }

        public void OnUseSkip(InputAction value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                _didUseSkip = true;
            }
        }
    }
}