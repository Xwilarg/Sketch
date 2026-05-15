using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Sketch.Translation
{
    public class TMP_TextTranslate : MonoBehaviour
    {
        private string _original;

        private void Start()
        {
            _original = GetComponent<TMP_Text>().text;
            UpdateText();
        }

        public void UpdateText()
        {
            var sentence = _original;
            foreach (var match in Regex.Matches(_original, "{([^}]+)}").Cast<Match>())
            {
                sentence = sentence.Replace(match.Value, Translate.Instance.Tr(match.Groups[1].Value));
            }
            GetComponent<TMP_Text>().text = sentence;
        }
    }
}