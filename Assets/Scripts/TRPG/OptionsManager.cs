using UnityEngine;

namespace Sketch.TRPG
{
    /// <summary>
    /// Manage all settings and display options
    /// </summary>
    public class OptionsManager : MonoBehaviour
    {
        public static OptionsManager Instance { private set; get; }

        [SerializeField]
        private GameObject _optionsContainer;

        public bool ShowLos { private set; get; } = true;

        private void Awake()
        {
            Instance = this;
        }

        public void ToggleOptionsContainer()
        {
            _optionsContainer.SetActive(!_optionsContainer.activeInHierarchy);
        }

        public void ToggleShowLos()
        {
            ShowLos = !ShowLos;
        }
    }
}
