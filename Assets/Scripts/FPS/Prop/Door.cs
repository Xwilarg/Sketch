using Sketch.FPS.Player;
using UnityEngine;

namespace Sketch.FPS.Prop
{
    public class Door : MonoBehaviour, IInteractable
    {
        private GameObject _me;

        private void Awake()
        {
            _me = gameObject;
        }

        public GameObject GameObject => _me;

        public string InteractionVerb => "FPS_open";

        public bool CanInteract(PlayerController pc)
            => true;

        public void Interact(PlayerController _)
        {
            _me = null;
            Destroy(gameObject);
        }
    }
}