using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Sketch.Translation
{
    public class TMP_TextTranslate : MonoBehaviour
    {
        [SerializeField, Tooltip("If ignored, force the TMP_Text to be translated in a specific language")]
        private bool _overrideTranslationLanguage;

        [SerializeField, Tooltip("If _overrideTranslationLanguage is enabled, force the TMP_Text to translate in this language")]
        private string _targetLanguage;

        private string _original;

        private void Start()
        {
            _original = GetComponent<TMP_Text>().text;
            if (_original == null)
            {
                Debug.LogError("TMP_TextTranslate must have a TMP_Text component attached");
            }
            else UpdateText();
        }

        public void UpdateText()
        {
            var sentence = _original;
            foreach (var match in Regex.Matches(_original, "{([^}]+)}").Cast<Match>())
            {
                if (_overrideTranslationLanguage) sentence = sentence.Replace(match.Value, Translate.Instance.TrWithLanguage(match.Groups[1].Value, _targetLanguage, Array.Empty<string>()));
                else sentence = sentence.Replace(match.Value, Translate.Instance.Tr(match.Groups[1].Value));
            }
            GetComponent<TMP_Text>().text = sentence;
        }
    }
}