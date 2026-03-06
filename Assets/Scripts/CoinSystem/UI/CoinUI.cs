using UnityEngine;
using TMPro;
using Platformer.Managers;

namespace KProject.CoinSystem
{
    public class CoinUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI coinText;

        void OnEnable()
        {
            CoinManager.OnCoinsChanged += UpdateText;
        }

        void OnDisable()
        {
            CoinManager.OnCoinsChanged -= UpdateText;
        }

        private void UpdateText(int amount)
        {
            coinText.text = amount.ToString();
        }
    }
}
