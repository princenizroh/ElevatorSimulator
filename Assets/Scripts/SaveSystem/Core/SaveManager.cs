using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using KProject.Core.Patterns;
using KProject.Core.Constants;
using KProject.UserAccountSystem;
using Platformer.Managers;
using Platformer;

namespace KProject.SaveSystem
{
    public class SaveManager : Singleton<SaveManager>
    {
        public SaveSlotInfo[] Slots { get; private set; }
        public int ActiveSlotIndex { get; private set; } = -1;
        private SaveData _pendingData = null;

        protected override void Awake()
        {
            base.Awake();
            InitSlots();
        }

        public void SetActiveSlot(int slotIndex)
        {
            ActiveSlotIndex = slotIndex;
        }

        public SaveResult AutoSave()
        {
            if (ActiveSlotIndex < 0) return SaveResult.InvalidSlot;
            return SaveToSlot(ActiveSlotIndex);
        }

        public bool HasAnySave()
        {
            if (Slots == null) return false;
            foreach (var slot in Slots)
                if (!slot.isEmpty) return true;
            return false;
        }

        void OnEnable()
        {
            UserAccountManager.OnUserChanged += HandleUserChanged;
            SceneManager.activeSceneChanged += HandleSceneChanged;
        }

        void OnDisable()
        {
            UserAccountManager.OnUserChanged -= HandleUserChanged;
            SceneManager.activeSceneChanged -= HandleSceneChanged;
        }

        void OnApplicationQuit()
        {
            AutoSave();
        }

        public SaveResult SaveToSlot(int slotIndex)
        {
            if (!IsValidSlot(slotIndex)) return SaveResult.InvalidSlot;
            if (UserAccountManager.Instance.ActiveUser == null) return SaveResult.NoActiveUser;

            SaveData data = CollectData();
            byte[] bytes = SaveSerializer.Serialize(data);
            string path = GetSlotPath(slotIndex);

            SaveResult result = SaveSerializer.WriteToDisk(path, bytes);

            if (result == SaveResult.Success)
            {
                Slots[slotIndex].isEmpty = false;
                Slots[slotIndex].savedAt = data.savedAt;
            }

            return result;
        }

        public SaveResult LoadFromSlot(int slotIndex)
        {
            if (!IsValidSlot(slotIndex)) return SaveResult.InvalidSlot;
            if (UserAccountManager.Instance.ActiveUser == null) return SaveResult.NoActiveUser;

            string path = GetSlotPath(slotIndex);
            var (result, data) = SaveSerializer.ReadFromDisk(path);

            if (result == SaveResult.Success)
            {
                if (CoinManager.Instance == null)
                    _pendingData = data;
                else
                    ApplyData(data);
            }

            return result;
        }

        public SaveResult DeleteSlot(int slotIndex)
        {
            if (!IsValidSlot(slotIndex)) return SaveResult.InvalidSlot;

            string path = GetSlotPath(slotIndex);
            if (!File.Exists(path)) return SaveResult.SlotEmpty;

            try
            {
                File.Delete(path);
                Slots[slotIndex].isEmpty = true;
                return SaveResult.Success;
            }
            catch
            {
                return SaveResult.Failed;
            }
        }

        public void RefreshSlotInfo()
        {
            if (UserAccountManager.Instance == null || UserAccountManager.Instance.ActiveUser == null)
            {
                InitSlots();
                return;
            }

            for (int i = 0; i < GameConstants.MAX_SAVE_SLOTS; i++)
            {
                string path = GetSlotPath(i);
                Slots[i].isEmpty = !File.Exists(path);

                if (!Slots[i].isEmpty)
                {
                    var (result, data) = SaveSerializer.ReadFromDisk(path);
                    if (result == SaveResult.Success)
                        Slots[i].savedAt = data.savedAt;
                }
            }
        }

        private SaveData CollectData()
        {
            var data = new SaveData();
            data.coins = (int)CoinManager.Instance.GetSaveData();
            data.collectedCoinIds = CoinManager.Instance.GetCollectedIds();

            var items = (List<ItemData>)InventoryManager.Instance.GetSaveData();
            foreach (var item in items)
                data.inventoryItemNames.Add(item.itemName);

            data.collectedItemIds = InventoryManager.Instance.GetCollectedItemIds();

            return data;
        }

        private void ApplyData(SaveData data)
        {
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.LoadSaveData(data.coins);
                CoinManager.Instance.SetCollectedIds(data.collectedCoinIds);
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.SetCollectedItemIds(data.collectedItemIds);

                var items = new List<ItemData>();
                foreach (string name in data.inventoryItemNames)
                {
                    var item = Resources.Load<ItemData>($"Game Data/Item/{name}");
                    if (item != null)
                        items.Add(item);
                    else
                        Debug.LogWarning($"[SaveManager] ItemData '{name}' not found in Resources/Data/");
                }
                InventoryManager.Instance.LoadSaveData(items);
            }
        }

        private void HandleUserChanged(UserAccountData user)
        {
            ActiveSlotIndex = -1;
            ResetGameData();
            RefreshSlotInfo();
        }

        private void ResetGameData()
        {
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.LoadSaveData(0);
                CoinManager.Instance.SetCollectedIds(new List<string>());
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.LoadSaveData(new List<ItemData>());
                InventoryManager.Instance.SetCollectedItemIds(new List<string>());
            }
        }

        private void HandleSceneChanged(Scene from, Scene to)
        {
            if (from.name == GameConstants.SCENE_GAMEPLAY)
                AutoSave();

            if (to.name == GameConstants.SCENE_GAMEPLAY && _pendingData != null)
            {
                ApplyData(_pendingData);
                _pendingData = null;
            }
        }

        private void InitSlots()
        {
            Slots = new SaveSlotInfo[GameConstants.MAX_SAVE_SLOTS];
            for (int i = 0; i < GameConstants.MAX_SAVE_SLOTS; i++)
                Slots[i] = new SaveSlotInfo(i);
        }

        private string GetSlotPath(int slotIndex)
        {
            string saveDir = UserAccountManager.Instance.GetActiveUserSavePath();
            return Path.Combine(saveDir, $"slot_{slotIndex}{GameConstants.SAVE_FILE_EXTENSION}");
        }

        private bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < GameConstants.MAX_SAVE_SLOTS;
        }
    }
}
