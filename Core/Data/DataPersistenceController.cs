using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Core.Data
{
    /// <summary>
    /// Handles Saving and Loading data
    /// </summary>
    public class DataPersistenceController : MonoSingleton<DataPersistenceController>
    {
        private List<IDataPersistable> allDataPersistables;
        private GameData gameData;

        [SerializeField] bool startWithoutLoad;

        public bool isNewGame { get; private set; }

        void Awake()
        {
            if (startWithoutLoad)
                NewGame();
            else 
                LoadGame();
        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
        }

        private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
        {
            
        }

        public void NewGame()
        {
            isNewGame = true;
            gameData = new GameData();
            allDataPersistables = FindAllPersistables();
        }
        public void SaveGame()
        {
            gameData = new();
            foreach (IDataPersistable dataPersistenceObj in allDataPersistables) 
            {
                dataPersistenceObj.OnSave(gameData);
            }
            ES3.Save("current", gameData);
        }

        public void LoadGame()
        {
            if (ES3.KeyExists("current"))
            {
                gameData = ES3.Load<GameData>("current");
            }
            else
            {
                NewGame();
                return;
            }

            allDataPersistables = FindAllPersistables();
            foreach (IDataPersistable dataPersistenceObj in allDataPersistables) 
            {
                dataPersistenceObj.OnLoad(gameData);
            }
        }

        private List<IDataPersistable> FindAllPersistables()
        {
            return new List<IDataPersistable>( FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistable>().ToList());
        }
    }
}