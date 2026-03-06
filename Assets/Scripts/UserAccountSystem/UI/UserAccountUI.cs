using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KProject.LocalizationSystem;


namespace KProject.UserAccountSystem
{
    public class UserAccountUI : MonoBehaviour
    {
        [Header("Save Slot Panel")]
        [SerializeField] private GameObject saveSlotPanel;

        [Header("Trigger")]
        [SerializeField] private Button openAccountListButton;
        [SerializeField] private Button openLoginPanelButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button logoutButton;

        [Header("Panel: Account List")]
        [SerializeField] private GameObject accountListPanel;
        [SerializeField] private Transform accountListContainer;
        [SerializeField] private GameObject accountItemPrefab;
        [SerializeField] private Button openCreatePanelButton;
        [SerializeField] private Button closeAccountListButton;
        
        [Header("Panel: Create Account")]
        [SerializeField] private GameObject createAccountPanel;
        [SerializeField] private TMP_InputField createUsernameInput;
        [SerializeField] private TMP_InputField createPasswordInput;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Button createConfirmButton;
        [SerializeField] private Button createCloseButton;

        [Header("Panel: Login")]
        [SerializeField] private GameObject loginAccountPanel;
        [SerializeField] private TMP_InputField loginUsernameInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private TextMeshProUGUI loginFeedbackText;
        [SerializeField] private Button loginConfirmButton;
        [SerializeField] private Button loginCloseButton;
        [Header("Active User")]
        [SerializeField] private TextMeshProUGUI activeUserText;

        void Awake()
        {
            accountListPanel.SetActive(false);
            createAccountPanel.SetActive(false);
            loginAccountPanel.SetActive(false);
            saveSlotPanel.SetActive(false);
            playButton.gameObject.SetActive(false);
            logoutButton.gameObject.SetActive(false);

            createPasswordInput.contentType = TMP_InputField.ContentType.Password;
            loginPasswordInput.contentType = TMP_InputField.ContentType.Password;
        }

        void OnEnable()
        {
            UserAccountManager.OnUserChanged += HandleUserChanged;

            openAccountListButton.onClick.AddListener(OpenAccountListPanel);
            openCreatePanelButton.onClick.AddListener(OpenCreatePanel);
            closeAccountListButton.onClick.AddListener(CloseAllPanel);
            createConfirmButton.onClick.AddListener(OnClickConfirmCreate);
            createCloseButton.onClick.AddListener(CloseAllPanel);
            loginConfirmButton.onClick.AddListener(OnClickConfirmLogin);
            loginCloseButton.onClick.AddListener(CloseAllPanel);
            openLoginPanelButton.onClick.AddListener(OpenLoginPanel);
            playButton.onClick.AddListener(OnClickPlay);
            logoutButton.onClick.AddListener(OnClickLogout);
        }

        void OnDisable()
        {
            UserAccountManager.OnUserChanged -= HandleUserChanged;

            openAccountListButton.onClick.RemoveListener(OpenAccountListPanel);
            openCreatePanelButton.onClick.RemoveListener(OpenCreatePanel);
            closeAccountListButton.onClick.RemoveListener(CloseAllPanel);
            createConfirmButton.onClick.RemoveListener(OnClickConfirmCreate);
            createCloseButton.onClick.RemoveListener(CloseAllPanel);
            loginConfirmButton.onClick.RemoveListener(OnClickConfirmLogin);
            loginCloseButton.onClick.RemoveListener(CloseAllPanel);
            openLoginPanelButton.onClick.RemoveListener(OpenLoginPanel);
            playButton.onClick.RemoveListener(OnClickPlay);
            logoutButton.onClick.RemoveListener(OnClickLogout);
        }

        public void OpenAccountListPanel()
        {
            createAccountPanel.SetActive(false);
            loginAccountPanel.SetActive(false);
            accountListPanel.SetActive(true);
            RefreshAccountList();
        }

        private void CloseAllPanel()
        {
            accountListPanel.SetActive(false);
            createAccountPanel.SetActive(false);
            loginAccountPanel.SetActive(false);
        }

        private void OpenCreatePanel()
        {
            accountListPanel.SetActive(false);
            createAccountPanel.SetActive(true);
            createUsernameInput.text = string.Empty;
            createPasswordInput.text = string.Empty;
            feedbackText.text = string.Empty;
        }

        private void OpenLoginPanel()
        {
            accountListPanel.SetActive(false);
            loginAccountPanel.SetActive(true);
            loginUsernameInput.text = string.Empty;
            loginPasswordInput.text = string.Empty;
            feedbackText.text = string.Empty;
        }

        private void OnClickConfirmCreate()
        {
            string username = createUsernameInput.text.Trim();
            string password = createPasswordInput.text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                feedbackText.text = LocalizationManager.Instance.GetText("err_username_empty");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                feedbackText.text = LocalizationManager.Instance.GetText("err_password_empty");
                return;
            }

            if (password.Length < 8)
            {
                feedbackText.text = LocalizationManager.Instance.GetText("err_password_short");
                return;
            }

            bool success = UserAccountManager.Instance.CreateAccount(username, password);

            if (success)
            {
                OpenAccountListPanel();
            }
            else
            {
                feedbackText.text = LocalizationManager.Instance.GetText("err_username_exists");
            }
        }

        private void OnClickConfirmLogin()
        {
            string username = loginUsernameInput.text.Trim();
            string password = loginPasswordInput.text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                loginFeedbackText.text = LocalizationManager.Instance.GetText("err_username_empty");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                loginFeedbackText.text = LocalizationManager.Instance.GetText("err_password_empty");
                return;
            }

            bool success = UserAccountManager.Instance.SwitchAccount(username, password);

            if (success)
            {
                loginAccountPanel.SetActive(false);
                playButton.gameObject.SetActive(true);
                logoutButton.gameObject.SetActive(true);
            }
            else
            {
                loginFeedbackText.text = LocalizationManager.Instance.GetText("err_wrong_credentials");
            }
        }

        private void OnClickPlay()
        {
            CloseAllPanel();
            saveSlotPanel.SetActive(true);
        }

        private void OnClickLogout()
        {
            UserAccountManager.Instance.Logout();
            playButton.gameObject.SetActive(false);
            logoutButton.gameObject.SetActive(false);
            saveSlotPanel.SetActive(false);
            CloseAllPanel();
        }

        private void OnClickDeleteAccount(string username)
        {
            UserAccountManager.Instance.DeleteAccount(username);
            RefreshAccountList();
        }

        private void RefreshAccountList()
        {
            foreach (Transform child in accountListContainer)
                Destroy(child.gameObject);

            List<UserAccountData> accounts = UserAccountManager.Instance.GetAllAccounts();

            foreach (var account in accounts)
            {
                GameObject item = Instantiate(accountItemPrefab, accountListContainer);
                string capturedUsername = account.username;

                item.GetComponentInChildren<TextMeshProUGUI>().text = capturedUsername;

                Button[] buttons = item.GetComponentsInChildren<Button>();
                buttons[0].onClick.AddListener(() => OnClickDeleteAccount(capturedUsername));
            }
        }

        private void HandleUserChanged(UserAccountData user)
        {
            activeUserText.text = user != null ? user.username : string.Empty;
        }
    }
}
