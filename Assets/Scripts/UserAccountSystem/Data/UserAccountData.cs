using System;

namespace KProject.UserAccountSystem
{
    [Serializable]
    public class UserAccountData
    {
        public string username;
        public string password;
        public DateTime createdAt;

        public UserAccountData(string username, string password)
        {
            this.username = username;
            this.password = password;
            this.createdAt = DateTime.Now;
        }
    }
}
