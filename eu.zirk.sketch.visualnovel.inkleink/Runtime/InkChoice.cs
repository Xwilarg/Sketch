using Ink.Runtime;

namespace Sketch.VN.InkleInk
{
    public class InkChoice : IChoice
    {
        public InkChoice(Choice choice)
        {
            Choice = choice;
        }

        public Choice Choice { get; }
        public string Text => Choice.text;
    }
}
