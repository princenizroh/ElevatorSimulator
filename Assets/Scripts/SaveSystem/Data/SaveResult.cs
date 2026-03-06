namespace KProject.SaveSystem
{
    public enum SaveResult
    {
        Success,
        Failed,
        NotEnoughSpace,
        SlotEmpty,
        FileCorrupted,
        NoActiveUser,
        InvalidSlot
    }
}
