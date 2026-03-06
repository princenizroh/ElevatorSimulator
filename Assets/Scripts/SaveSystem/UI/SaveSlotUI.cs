using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using KProject.Core.Constants;
using KProject.UserAccountSystem;
using KProject.LocalizationSystem;

namespace KProject.SaveSystem
{
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("Slots")]
        [SerializeField] private Button[] slotButtons;

        [Header("Popup")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TextMeshProUGUI popupSlotText;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button cancelButton;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;

        private int _selectedSlot = -1;

        void Awake()
        {
            popupPanel.SetActive(false);

            for (int i = 0; i < GameConstants.MAX_SAVE_SLOTS; i++)
            {
                int captured = i;
                slotButtons[i].onClick.AddListener(() => OnClickSlot(captured));
            }

            loadButton.onClick.AddListener(OnClickLoad);
            deleteButton.onClick.AddListener(OnClickDelete);
            cancelButton.onClick.AddListener(() => popupPanel.SetActive(false));
        }

        void OnEnable()
        {
            feedbackText.text = string.Empty;
            UserAccountManager.OnUserChanged += HandleUserChanged;
            RefreshUI();
        }

        void OnDisable()
        {
            UserAccountManager.OnUserChanged -= HandleUserChanged;
        }

        private void HandleUserChanged(UserAccountData user)
        {
            RefreshUI();
        }

        private void OnClickSlot(int slotIndex)
        {
            if (SaveManager.Instance.Slots[slotIndex].isEmpty)
            {
                SaveManager.Instance.SetActiveSlot(slotIndex);
                SceneManager.LoadScene(GameConstants.SCENE_GAMEPLAY);
            }
            else
            {
                _selectedSlot = slotIndex;
                SaveSlotInfo slot = SaveManager.Instance.Slots[slotIndex];
                popupSlotText.text = $"Slot {slotIndex + 1}";
                popupPanel.SetActive(true);
            }
        }

        private void OnClickLoad()
        {
            SaveResult result = SaveManager.Instance.LoadFromSlot(_selectedSlot);

            if (result == SaveResult.Success)
            {
                SaveManager.Instance.SetActiveSlot(_selectedSlot);
                popupPanel.SetActive(false);
                SceneManager.LoadScene(GameConstants.SCENE_GAMEPLAY);
            }
            else
            {
                feedbackText.text = result switch
                {
                    SaveResult.FileCorrupted => LocalizationManager.Instance.GetText("err_save_corrupted"),
                    SaveResult.NoActiveUser => LocalizationManager.Instance.GetText("err_save_no_user"),
                    _ => LocalizationManager.Instance.GetText("err_load_failed")
                };
                popupPanel.SetActive(false);
            }
        }

        private void OnClickDelete()
        {
            SaveResult result = SaveManager.Instance.DeleteSlot(_selectedSlot);

            feedbackText.text = result switch
            {
                SaveResult.Success => LocalizationManager.Instance.GetText("slot_deleted"),
                SaveResult.SlotEmpty => LocalizationManager.Instance.GetText("slot_empty"),
                _ => LocalizationManager.Instance.GetText("err_save_failed")
            };

            popupPanel.SetActive(false);
            _selectedSlot = -1;
            RefreshUI();
        }

        private void RefreshUI()
        {
            bool hasManager = SaveManager.Instance != null;
            if (hasManager) SaveManager.Instance.RefreshSlotInfo();

            for (int i = 0; i < GameConstants.MAX_SAVE_SLOTS; i++)
            {
                var label = slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (label == null) continue;

                if (!hasManager || SaveManager.Instance.Slots[i].isEmpty)
                {
                    label.text = $"Slot {i + 1}\n{LocalizationManager.Instance?.GetText("slot_empty") ?? "Empty"}";
                }
                else
                {
                    SaveSlotInfo slot = SaveManager.Instance.Slots[i];
                    label.text = $"Slot {i + 1}\n{slot.savedAt:dd/MM/yyyy HH:mm}";
                }
            }
        }
    }
}
