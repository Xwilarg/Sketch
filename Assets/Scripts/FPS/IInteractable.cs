using Sketch.FPS;
using UnityEngine;

namespace Sketch.Player
{
    public interface IInteractable
    {
        public GameObject GameObject { get; }
        public bool CanInteract(PlayerController pc);
        public void Interact(PlayerController pc);
    }
}
