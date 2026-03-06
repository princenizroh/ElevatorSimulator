namespace KProject.Core.Interfaces
{
    public interface ISaveable
    {
        object GetSaveData();
        void LoadSaveData(object data);
    }
}
