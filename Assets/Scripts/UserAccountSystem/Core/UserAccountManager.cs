using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using KProject.Core.Patterns;
using KProject.Core.Constants;

namespace KProject.UserAccountSystem
{
    public class UserAccountManager : Singleton<UserAccountManager>
    {
        public static event Action<UserAccountData> OnUserChanged;

        public UserAccountData ActiveUser { get; private set; }

        private List<UserAccountData> _accounts = new List<UserAccountData>();
        private string _usersFilePath;

        protected override void Awake()
        {
            base.Awake();
            _usersFilePath = Path.Combine(Application.persistentDataPath, GameConstants.USERS_FILE);
            LoadAccounts();
        }

        public List<UserAccountData> GetAllAccounts() => new List<UserAccountData>(_accounts);

        public bool CreateAccount(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            if (string.IsNullOrWhiteSpace(password)) return false;
            if (_accounts.Exists(a => a.username == username)) return false;
            _accounts.Add(new UserAccountData(username, password));
            SaveAccounts();
            return true;
        }

        public bool DeleteAccount(string username)
        {
            var account = _accounts.Find(a => a.username == username);
            if (account == null)
                return false;

            DeleteUserSaveFolder(username);
            _accounts.Remove(account);
            SaveAccounts();

            if (ActiveUser != null && ActiveUser.username == username)
            {
                ActiveUser = null;
                OnUserChanged?.Invoke(null);
            }

            return true;
        }

        public bool SwitchAccount(string username, string password)
        {
            var account = _accounts.Find(a => a.username == username && a.password == password);
            if (account == null)
                return false;

            ActiveUser = account;
            OnUserChanged?.Invoke(ActiveUser);
            return true;
        }

        public void Logout()
        {
            ActiveUser = null;
            OnUserChanged?.Invoke(null);
        }

        public string GetActiveUserSavePath()
        {
            if (ActiveUser == null)
                return null;

            string path = Path.Combine(
                Application.persistentDataPath,
                GameConstants.USERS_FOLDER,
                ActiveUser.username,
                GameConstants.SAVE_FOLDER
            );

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        private void DeleteUserSaveFolder(string username)
        {
            string path = Path.Combine(
                Application.persistentDataPath,
                GameConstants.USERS_FOLDER,
                username
            );

            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        private void SaveAccounts()
        {
            try
            {
                using var stream = new FileStream(_usersFilePath, FileMode.Create);
                var formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
                formatter.Serialize(stream, _accounts);
#pragma warning restore SYSLIB0011
            }
            catch (Exception e)
            {
                Debug.LogError($"[UserAccountManager] Failed to save accounts: {e.Message}");
            }
        }

        private void LoadAccounts()
        {
            if (!File.Exists(_usersFilePath))
                return;

            try
            {
                using var stream = new FileStream(_usersFilePath, FileMode.Open);
                var formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
                _accounts = (List<UserAccountData>)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
            }
            catch (Exception e)
            {
                Debug.LogError($"[UserAccountManager] Failed to load accounts: {e.Message}");
                _accounts = new List<UserAccountData>();
            }
        }
    }
}
