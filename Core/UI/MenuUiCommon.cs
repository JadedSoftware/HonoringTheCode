using Core.Audio;
using Core.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Core.NonLevel_Scenes
{
    public enum ButtonTypes
    {
        Start
    }
    public class MenuUiCommon : MonoBehaviour
    {
        protected Button startButton;
        protected VisualElement root;
        [SerializeField] protected SceneTypes nextScene;


        public bool isAsync;
        private string startGameButton => UiCommonStrings.startGameButton;

        protected virtual void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            startButton = root.Q<Button>(startGameButton);
            startButton.clicked += StartButtonClicked;
            if (isAsync)
            {
                SceneLoader.LoadAsync(nextScene);
            }
        }

        protected virtual void StartButtonClicked()
        {
            AudioController.instance.PlayButtonSound(ButtonTypes.Start);
            SceneLoader.Load(nextScene);
        }
    }
}