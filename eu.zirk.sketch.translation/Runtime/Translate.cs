using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Sketch.Translation
{
    public class Translate
    {
        /// <summary>
        /// Languages available in the game
        /// </summary>
        public static string[] Languages { private set; get; } = new string[] { "english" };
        /// <summary>
        /// Where all files are located in Resources/
        /// </summary>
        private static Dictionary<string, string> _paths = new();

        /// <summary>
        /// Apply a modification on a translation
        /// </summary>
        public Func<string, string> TranslationHook;

        /// <summary>
        /// Called when the current language is changed
        /// </summary>
        public UnityEvent OnLanguageChanged { get; } = new();

        private Translate()
        {
            UpdateTranslations();
        }

        private static Translate _instance;
        public static Translate Instance
        {
            private set => _instance = value;
            get
            {
                _instance ??= new Translate();
                return _instance;
            }
        }

        public void SetLanguages(string[] overrideLanguages)
        {
            if (overrideLanguages != null) Languages = overrideLanguages;

            UpdateTranslations();
        }
        public void SetLanguages(Dictionary<string, string> overrideLanguages)
        {
            if (overrideLanguages != null)
            {
                Languages = overrideLanguages.Keys.ToArray();
                _paths = overrideLanguages;
            }

            UpdateTranslations();
        }

        private void UpdateTranslations()
        {
            _translationData.Clear();
            foreach (var lang in Languages)
            {
                _translationData.Add(lang, JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    Resources.Load<TextAsset>(_paths.TryGetValue(lang, out var path) ? path : lang).text
                ));
            }
        }

        public bool Exists(string key) => _translationData["english"].ContainsKey(key);

        public string Tr(string key, params string[] arguments)
        {
            return TrWithLanguage(key, _currentLanguage, arguments);
        }

        public string TrWithLanguage(string key, string language, string[] arguments)
        {
            var langData = _translationData[language];
            string sentence;
            if (langData.ContainsKey(key))
            {
                sentence = langData[key];
            }
            else if (_translationData["english"].TryGetValue(key, out var value))
            {
                Debug.LogWarning($"[Translation] Impossible to find key {key} in {language}");
                sentence = _translationData["english"][key];
            }
            else
            {
                Debug.LogError($"[Translation] Impossible to find key {key}");
                sentence = key;
            }
            for (int i = 0; i < arguments.Length; i++)
            {
                sentence = sentence.Replace("{" + i + "}", arguments[i]);
            }

            if (TranslationHook != null) return TranslationHook(sentence);

            return sentence;
        }

        private string _currentLanguage = "english";
        public string CurrentLanguage
        {
            set
            {
                if (!_translationData.ContainsKey(value))
                {
                    throw new ArgumentException($"Invalid translation key {value}", nameof(value));
                }
                _currentLanguage = value;
                foreach (var tt in UnityEngine.Object.FindObjectsByType<TMP_TextTranslate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    tt.UpdateText();
                }
                OnLanguageChanged.Invoke();
            }
            get => _currentLanguage;
        }

        private readonly Dictionary<string, Dictionary<string, string>> _translationData = new();
    }
}