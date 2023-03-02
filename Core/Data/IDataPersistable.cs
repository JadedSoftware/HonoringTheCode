using Core.Data;

namespace Core.Interfaces
{
    /// <summary>
    /// An interface for objects that need saving and loading 
    /// </summary>
    public interface IDataPersistable
    {
        void OnSave(GameData gameData);
        void OnLoad(GameData gameData);
    }
}