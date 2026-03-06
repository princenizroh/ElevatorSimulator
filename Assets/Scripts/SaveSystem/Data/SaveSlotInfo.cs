using System;

namespace KProject.SaveSystem
{
    [Serializable]
    public class SaveSlotInfo
    {
        public int slotIndex;
        public bool isEmpty;
        public DateTime savedAt;

        public SaveSlotInfo(int slotIndex)
        {
            this.slotIndex = slotIndex;
            this.isEmpty = true;
        }
    }
}
