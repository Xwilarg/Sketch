using UnityEngine;

namespace Sketch.Generation
{
    /// <summary>
    /// Manage all settings and display options
    /// </summary>
    public class OptionsManager : MonoBehaviour
    {
        public static OptionsManager Instance { private set; get; }

        [SerializeField]
        private GameObject _optionsContainer;

        public bool ShowLinks { private set; get; } = true;
        public bool CalculateNewRooms { private set; get; } = true;

        private void Awake()
        {
            Instance = this;
        }

        public void ToggleOptionsContainer()
        {
            _optionsContainer.SetActive(!_optionsContainer.activeInHierarchy);
        }

        public void ToggleShowLinks()
        {
            ShowLinks = !ShowLinks;
            MapGenerator.Instance.ToggleAllLinks(ShowLinks);
        }

        public void ToggleCalculateNewRooms()
        {
            CalculateNewRooms = !CalculateNewRooms;
        }
    }
}
