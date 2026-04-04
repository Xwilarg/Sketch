using Sketch.Translation;
using UnityEngine;

namespace Sketch.FPS
{
    public class TranslatedPlayerController : PlayerController
    {
        private Vector3 _baseSpawnPos;

        protected override void Awake()
        {
            base.Awake();

            _baseSpawnPos = transform.position;
        }

        protected override void Update()
        {
            base.Update();

            if (transform.position.y < -10f)
            {
                transform.position = _baseSpawnPos;
                ResetGravity();
            }
        }

        public override string GetInteractionText(string interactionVerb)
        {
            return Translate.Instance.Tr("FPS_interactionText", Translate.Instance.Tr(interactionVerb));
        }

        public override string GetDenyText(string denySentence)
        {
            return Translate.Instance.Tr(denySentence);
        }
    }
}