using Sketch.Translation;

namespace Sketch.FPS
{
    public class TranslatedPlayerController : PlayerController
    {
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