using Core.Data;
using Core.Interfaces;
using UnityEngine;

namespace Core.GameManagement
{
    /// <summary>
    /// Handles level specific interactions
    /// </summary>
    public class LevelManagementController : MonoBehaviour, IDataPersistable

    {
        private static LevelManagementController _instance;
        public SelectableTypes startingTurn;

        public static LevelManagementController instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(LevelManagementController)) as LevelManagementController;

                return _instance;
            }
            set => _instance = value;
        }

        public void OnSave(GameData gameData)
        {
        }

        public void OnLoad(GameData gameData)
        {
            startingTurn = gameData.currentTurn;
        }
    }
}