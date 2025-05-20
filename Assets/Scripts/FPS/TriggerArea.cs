using UnityEngine;
using UnityEngine.Events;

namespace Sketch.FPS
{
    public class TriggerArea : MonoBehaviour
    {
        public UnityEvent<Collider> OnTriggerEnterEvent { get; } = new();
        public UnityEvent<Collider> OnTriggerExitEvent { get; } = new();

        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEnterEvent.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            OnTriggerExitEvent.Invoke(other);
        }
    }
}