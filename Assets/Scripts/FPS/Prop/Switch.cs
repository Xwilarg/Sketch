using Sketch.FPS.Player;
using UnityEngine;

namespace Sketch.FPS.Prop
{
    public class Switch : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private Material _highlightMat;
        public Renderer Renderer { private set; get; }

        public bool IsOn { private set; get; }

        private void Awake()
        {
            Renderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            FPSManager.Instance.Register(this);
        }

        public GameObject GameObject => gameObject;

        public string InteractionVerb => "FPS_activateSwitch";

        public string DenySentence => null;

        public bool CanInteract(PlayerController pc)
            => !IsOn;

        public void Interact(PlayerController pc)
        {
            Renderer.material = _highlightMat;
            IsOn = true;
        }
    }
}