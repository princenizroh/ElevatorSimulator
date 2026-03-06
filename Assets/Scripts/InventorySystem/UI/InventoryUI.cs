using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Platformer
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject itemSlotPrefab;

        private PlayerInputActions _inputActions;

        void Awake()
        {
            _inputActions = new PlayerInputActions();
            inventoryPanel.SetActive(false);
        }

        void OnEnable()
        {
            _inputActions.Player.Enable();
            _inputActions.Player.Inventory.performed += ToggleInventory;
            InventoryManager.OnItemsChanged += RefreshSlots;
        }

        void OnDisable()
        {
            _inputActions.Player.Inventory.performed -= ToggleInventory;
            _inputActions.Player.Disable();
            InventoryManager.OnItemsChanged -= RefreshSlots;
        }

        private void ToggleInventory(InputAction.CallbackContext context)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }

        private void RefreshSlots(List<ItemData> items)
        {
            foreach (Transform child in itemsContainer)
                Destroy(child.gameObject);

            foreach (var item in items)
            {
                GameObject slot = Instantiate(itemSlotPrefab, itemsContainer);
                slot.GetComponent<Image>().sprite = item.itemIcon;
            }
        }
    }
}
