using System;
using System.Collections.Generic;

namespace Sketch.VN
{
    public class TextStory : IStory
    {
        private string[] _dialogues;
        private int _index;

        public TextStory(string[] dialogues)
        {
            _dialogues = dialogues;
            _index = 0;
        }

        public IEnumerable<IChoice> Choices => Array.Empty<IChoice>();

        public IEnumerable<string> CurrentTags => Array.Empty<string>();

        public bool CanContinue => _index < _dialogues.Length;

        public void ChoosePath(IChoice choice)
        {
            throw new System.NotImplementedException();
        }

        public string Continue()
        {
            var text = _dialogues[_index];
            _index++;
            return text;
        }
    }
}
