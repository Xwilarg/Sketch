using Ink.Runtime;
using Ink.UnityIntegration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sketch.VN.InkleInk
{
    public class InkStory : IStory
    {
        public InkStory(InkFile inkFile, Action<VariablesState> updateVariables = null)
        {
            _story = new(inkFile.storyJson);
            updateVariables?.Invoke(_story.variablesState);
        }

        private Story _story;

        public IEnumerable<IChoice> Choices => _story.currentChoices.Select(x => new InkChoice(x));

        public IEnumerable<string> CurrentTags => _story.currentTags;

        public bool CanContinue => _story.canContinue;

        public string Continue()
        {
            return _story.Continue();
        }

        public void ChoosePath(IChoice choice)
        {
            _story.ChoosePath((((InkChoice)choice).Choice).targetPath);
        }
    }
}
