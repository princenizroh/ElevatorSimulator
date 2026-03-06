using UnityEngine;
using TMPro;

namespace KProject.LocalizationSystem
{
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;

        private TextMeshProUGUI _text;

        void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += UpdateText;
            UpdateText();
        }

        void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= UpdateText;
        }

        private void UpdateText()
        {
            if (LocalizationManager.Instance == null) return;

            _text.text = LocalizationManager.Instance.GetText(key);

            var font = LocalizationManager.Instance.GetCurrentFont();
            if (font != null)
                _text.font = font;
        }
    }
}
