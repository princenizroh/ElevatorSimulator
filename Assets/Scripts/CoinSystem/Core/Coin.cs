using UnityEngine;

namespace Platformer.Managers
{
    public class Coin : MonoBehaviour
    {
        [SerializeField] private int coinValue = 1;
        private string _id;

        void Awake()
        {
            _id = $"{gameObject.scene.name}_{transform.position.x:F1}_{transform.position.y:F1}";
        }

        void Start()
        {
            if (CoinManager.Instance != null && CoinManager.Instance.IsCollected(_id))
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                CoinManager.Instance.AddCoins(coinValue, _id);
                Destroy(gameObject);
            }
        }
    }
}
