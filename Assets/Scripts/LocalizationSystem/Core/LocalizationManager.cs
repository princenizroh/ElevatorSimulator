using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using KProject.Core.Patterns;

namespace KProject.LocalizationSystem
{
    public class LocalizationManager : Singleton<LocalizationManager>
    {
        public static event Action OnLanguageChanged;

        public LanguageCode CurrentLanguage { get; private set; } = LanguageCode.EN;

        [SerializeField] private LocalizationFontConfig fontConfig;

        private Dictionary<string, string> _entries = new Dictionary<string, string>();
        private string _languageFilePath;

        private static readonly Dictionary<LanguageCode, string> _fileNames = new Dictionary<LanguageCode, string>
        {
            { LanguageCode.EN,   "en.json"   },
            { LanguageCode.RU,   "ru.json"   },
            { LanguageCode.ZH_S, "zh-s.json" },
            { LanguageCode.ZH_T, "zh-t.json" },
            { LanguageCode.JA,   "ja.json"   }
        };

        protected override void Awake()
        {
            base.Awake();
            _languageFilePath = Path.Combine(Application.persistentDataPath, "language.dat");
            CurrentLanguage = LoadLanguagePreference();
        }

        void Start()
        {
            StartCoroutine(LoadLanguage(CurrentLanguage));
        }

        public void SetLanguage(LanguageCode code)
        {
            CurrentLanguage = code;
            SaveLanguagePreference(code);
            StartCoroutine(LoadLanguage(code));
        }

        public string GetText(string key)
        {
            if (_entries.TryGetValue(key, out string value))
                return value;
            return $"[{key}]";
        }

        public TMP_FontAsset GetCurrentFont()
        {
            if (fontConfig == null) return null;
            return fontConfig.GetFont(CurrentLanguage);
        }

        private void SaveLanguagePreference(LanguageCode code)
        {
            try
            {
                using var stream = new FileStream(_languageFilePath, FileMode.Create);
                var formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
                formatter.Serialize(stream, (int)code);
#pragma warning restore SYSLIB0011
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationManager] Failed to save language: {e.Message}");
            }
        }

        private LanguageCode LoadLanguagePreference()
        {
            if (!File.Exists(_languageFilePath))
                return LanguageCode.EN;

            try
            {
                using var stream = new FileStream(_languageFilePath, FileMode.Open);
                var formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
                return (LanguageCode)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationManager] Failed to load language: {e.Message}");
                return LanguageCode.EN;
            }
        }

        private IEnumerator LoadLanguage(LanguageCode code)
        {
            string fileName = _fileNames[code];
            string path = Path.Combine(Application.streamingAssetsPath, "Localization", fileName);

#if UNITY_ANDROID && !UNITY_EDITOR
            using var request = UnityWebRequest.Get(path);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[LocalizationManager] Failed to load {fileName}: {request.error}");
                yield break;
            }

            ParseJson(request.downloadHandler.text);
#else
            if (!File.Exists(path))
            {
                Debug.LogError($"[LocalizationManager] File not found: {path}");
                yield break;
            }

            string json = File.ReadAllText(path);
            ParseJson(json);
            yield return null;
#endif

            OnLanguageChanged?.Invoke();
        }

        private void ParseJson(string json)
        {
            _entries.Clear();
            var wrapper = JsonUtility.FromJson<LocalizationWrapper>(json);
            if (wrapper?.entries == null) return;

            foreach (var entry in wrapper.entries)
                _entries[entry.key] = entry.value;
        }

        [Serializable]
        private class LocalizationWrapper
        {
            public List<LocalizationEntry> entries;
        }

        [Serializable]
        private class LocalizationEntry
        {
            public string key;
            public string value;
        }
    }
}
