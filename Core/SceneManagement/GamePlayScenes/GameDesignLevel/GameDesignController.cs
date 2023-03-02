using Core.Audio;
using Core.GameManagement;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

namespace Core.SceneManagement.GamePlayScenes.GameDesignLevel
{
    /// <summary>
    /// Handles events specific to the "Game Design" Level,
    /// </summary>
    public class GameDesignController : LevelController
    {
        public void OnEnable()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }
        
        private void RegisterEvents()
        {
            EventSenderController.onFadeIn += SceneFadedIn;
            EventSenderController.onNavReady += NavReady;
        }
        private void UnRegisterEvents()
        {
            EventSenderController.onFadeIn -= SceneFadedIn;
            EventSenderController.onNavReady += NavReady;
        }

        private void NavReady()
        {
            AudioController.instance.ChangePlaylist(PlayListTypes.GameDesign);
        }

        private void SceneFadedIn()
        {
            
        }
    }
}