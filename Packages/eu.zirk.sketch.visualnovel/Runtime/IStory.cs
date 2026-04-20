using System.Collections.Generic;

namespace Sketch.VN
{
    public interface IStory
    {
        public IEnumerable<IChoice> Choices { get; }

        public IEnumerable<string> CurrentTags { get; }

        public void ChoosePath(IChoice choice);

        public bool CanContinue { get; }

        public string Continue();
    }
}
