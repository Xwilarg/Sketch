using UnityEngine;

namespace Sketch.FPS.Player
{
    public interface IInteractable
    {
        public GameObject GameObject { get; }
        public bool CanInteract(PlayerController pc);
        public void Interact(PlayerController pc);

        public string InteractionVerb { get; }
    }
}
