using UnityEngine;

namespace Platformer
{
    public class Item : MonoBehaviour
    {
        [SerializeField] private ItemData itemData;
        private string _id;

        void Awake()
        {
            _id = $"{gameObject.scene.name}_{transform.position.x:F1}_{transform.position.y:F1}";
        }

        void Start()
        {
            if (InventoryManager.Instance != null && InventoryManager.Instance.IsCollected(_id))
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                InventoryManager.Instance.AddItem(itemData, _id);
                Destroy(gameObject);
            }
        }
    }
}