using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace KProject.LocalizationSystem
{
    [CreateAssetMenu(fileName = "LocalizationFontConfig", menuName = "KProject/Localization Font Config")]
    public class LocalizationFontConfig : ScriptableObject
    {
        [Serializable]
        public class LanguageFontPair
        {
            public LanguageCode language;
            public TMP_FontAsset font;
        }

        [SerializeField] private List<LanguageFontPair> fontMap;

        public TMP_FontAsset GetFont(LanguageCode code)
        {
            foreach (var pair in fontMap)
                if (pair.language == code) return pair.font;
            return null;
        }
    }
}
