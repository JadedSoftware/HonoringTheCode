using Core.Data;

namespace Core.GameManagement.EventSenders
{
    /// <summary>
    /// Handles all event requests 
    /// </summary>
    public static partial class EventSenderController
    {

        // ----------- Setup -------------------//

        public delegate void OnGridConfigured();

        public static event OnGridConfigured gridConfigured;

        public delegate void OnNavEstablished();

        public static event OnNavEstablished onNavEstablished;

        public delegate void OnNavBuilt();

        public static event OnNavBuilt onNavReady;

        public delegate void OnFadeIn();

        public static event OnFadeIn onFadeIn;

        public delegate void OnSave(GameData gameData);

        public static event OnSave onSave;

        public delegate void OnLoad(GameData gameData);

        public static event OnLoad onLoad;


        //--------------- Static Methods ---------------//

        public static void GridConfigured()
        {
            ScheduleEvent(() => gridConfigured?.Invoke());
        }

        public static void NavlinksEstablished()
        {
            ScheduleEvent(() => onNavEstablished?.Invoke());
        }

        public static void NavPathBuilt()
        {
            ScheduleEvent(() => onNavReady?.Invoke());
        }

        public static void SceneFadedIn()
        {
            ScheduleEvent(() => onFadeIn?.Invoke());
        }

        public static void Save(GameData gameData)
        {
            ScheduleEvent(() => onSave?.Invoke(gameData));
        }

        public static void Load(GameData gameData)
        {
            ScheduleEvent(() => onLoad?.Invoke(gameData));
        }
    }
}