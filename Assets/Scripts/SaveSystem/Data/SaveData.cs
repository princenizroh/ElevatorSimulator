using System;
using System.Collections.Generic;

namespace KProject.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public int coins;
        public List<string> inventoryItemNames = new List<string>();
        public List<string> collectedCoinIds = new List<string>();
        public List<string> collectedItemIds = new List<string>();
        public DateTime savedAt;

        public SaveData()
        {
            savedAt = DateTime.Now;
        }
    }
}
