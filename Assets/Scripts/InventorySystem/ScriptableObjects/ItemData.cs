using UnityEngine;

namespace Platformer
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Game Data/Item/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public Sprite itemIcon;
        public int value;
    }
}
