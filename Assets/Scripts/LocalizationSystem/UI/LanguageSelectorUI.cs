using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KProject.LocalizationSystem
{
    public class LanguageSelectorUI : MonoBehaviour
    {
        [SerializeField] private Button[] languageButtons;

        private static readonly LanguageCode[] _order = new LanguageCode[]
        {
            LanguageCode.EN,
            LanguageCode.RU,
            LanguageCode.ZH_S,
            LanguageCode.ZH_T,
            LanguageCode.JA
        };

        void Awake()
        {
            for (int i = 0; i < languageButtons.Length && i < _order.Length; i++)
            {
                int captured = i;
                languageButtons[i].onClick.AddListener(() =>
                    LocalizationManager.Instance.SetLanguage(_order[captured]));
            }
        }
    }
}
