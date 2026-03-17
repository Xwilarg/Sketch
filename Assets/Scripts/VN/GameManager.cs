using Sketch.Achievement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Sketch.VN
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private TextAsset _introStory;

        private bool _didUseSkip;

        private void Start()
        {
            VNManager.Instance.ShowStory(_introStory,
                onDone: () => { SceneManager.LoadScene("Main"); },
                onTags: (tag, value) =>
                {
                    if (tag == "ach-noskip")
                    {
                        if (!_didUseSkip)
                        {
                            AchievementManager.Instance.Unlock(AchievementID.VIS_NoSkip);
                        }
                        return true;
                    }
                    return false;
                }
            );
        }

        public void OnUseSkip(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                _didUseSkip = true;
            }
        }
        public void OnUseSkipBtn()
        {
            _didUseSkip = true;
        }
    }
}