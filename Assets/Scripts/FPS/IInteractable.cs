using UnityEngine;

namespace Sketch.FPS
{
    public interface IInteractable
    {
        public GameObject GameObject { get; }
        public bool CanInteract(PlayerController pc);
        public void Interact(PlayerController pc);

        /// <summary>
        /// Verb shown in the format "Press 'E' to {verb}
        /// </summary>
        public string InteractionVerb { get; }
        /// <summary>
        /// Shown when we can't interact with the object, can be null
        /// </summary>
        public string DenySentence { get; }
    }
}
