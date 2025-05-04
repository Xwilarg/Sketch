using Sketch.Player;
using UnityEngine;

namespace Sketch.FPS
{
    public class Switch : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private Material _highlightMat;
        private Material _defaultMat;
        public Renderer Renderer { private set; get; }

        private void Awake()
        {
            Renderer = GetComponent<Renderer>();
            _defaultMat = Renderer.material;
        }

        public GameObject GameObject => gameObject;

        public bool CanInteract(PlayerController pc)
            => FPSManager.Instance.ActiveSwitch == null || FPSManager.Instance.ActiveSwitch.gameObject.GetInstanceID() != gameObject.GetInstanceID();

        public void Interact(PlayerController pc)
        {
            if (FPSManager.Instance.ActiveSwitch != null)
            {
                FPSManager.Instance.ActiveSwitch.Renderer.material = _defaultMat;
            }
            Renderer.material = _highlightMat;
            FPSManager.Instance.ActiveSwitch = this;

        }
    }
}