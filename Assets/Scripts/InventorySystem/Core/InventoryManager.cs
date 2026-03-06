using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using KProject.Core.Patterns;
using KProject.Core.Interfaces;

namespace Platformer
{
    public class InventoryManager : Singleton<InventoryManager>, ISaveable
    {
        public static event Action<List<ItemData>> OnItemsChanged;

        private List<ItemData> _items = new List<ItemData>();
        private HashSet<string> _collectedItemIds = new HashSet<string>();

        public void AddItem(ItemData itemData, string itemId = null)
        {
            _items.Add(itemData);
            if (itemId != null) _collectedItemIds.Add(itemId);
            OnItemsChanged?.Invoke(new List<ItemData>(_items));
        }

        public bool IsCollected(string itemId) => _collectedItemIds.Contains(itemId);

        public List<string> GetCollectedItemIds() => new List<string>(_collectedItemIds);

        public void SetCollectedItemIds(List<string> ids)
        {
            _collectedItemIds = new HashSet<string>(ids);
        }

        public object GetSaveData() => new List<ItemData>(_items);

        public void LoadSaveData(object data)
        {
            _items = (List<ItemData>)data;
            OnItemsChanged?.Invoke(new List<ItemData>(_items));
        }
    }
}
