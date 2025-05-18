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

        public string InteractionVerb => "FPS_openDoor";

        public string DenySentence => "FPS_doorClosed";

        public bool CanInteract(PlayerController pc)
            => FPSManager.Instance.AreAllSwitchesActive;

        public void Interact(PlayerController _)
        {
            _me = null;
            Destroy(gameObject);
        }
    }
}