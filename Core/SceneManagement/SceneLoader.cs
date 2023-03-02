using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.SceneManagement
{
    public enum SceneTypes{
        MainMenu,
        Intro,
        GameDesign
    }

    public enum SceneTransitionTypes
    {
        FadeToBlack,
        CrossFade,
    }
    public static class SceneLoader
    {
        public static float sceneProgress = 0;
        public static bool isSceneLoaded;
        private static AsyncOperation scene;

        public static void Load(SceneTypes nextScene)
        {
            SceneManager.LoadScene(nextScene.ToString());
        }

        public static async void LoadAsync(SceneTypes nextScene)
        {
            scene = SceneManager.LoadSceneAsync(nextScene.ToString());
            scene.allowSceneActivation = false;
            isSceneLoaded = false;
            do
            {
                sceneProgress = scene.progress;
                await Task.Delay(100);
            } while (scene.progress < 0.9f);
            isSceneLoaded = true;
        }

        public static void AllowSceneLoad(bool isActivationAllowed)
        {
            scene.allowSceneActivation = isActivationAllowed;
        }
    }
}