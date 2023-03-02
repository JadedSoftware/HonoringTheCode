using System.Collections;
using Core.Audio;
using Core.NonLevel_Scenes;
using Core.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Core.IntroScene
{
    public class IntroSceneUi : MenuUiCommon
    {
        public const string introText =
            "The ever-churning cogs of galactic warfare have heavily strained the resources of the Joht alliance. " +
            "\n \n" +
            "Two members of the Johthun special forces team have been dispatched to the distant world of Dimoria, a cold planet composed of red sand seas and endless desert. " +
            "A nearby passing probe has detected energy signatures that indicate the rare mineral Berembian could be hidden somewhere between the dirt and rocks. " +
            "\n \n" +
            "Recently recruited Tazlow Raff and the well establshed Colonel Lazure Lans venture to the planet's surface, hoping to claim the un-synthesizable mineral for the glory of Joht.";


        public const string introTextLabel = "IntroText";
        private Label introLabel;
        private ProgressBar progressBar;
        
        private static IntroSceneUi _instance;
        public static IntroSceneUi instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(IntroSceneUi)) as IntroSceneUi;
                return _instance;
            }
            set => _instance = value;
        }
       
        protected override void Start()
        {
            isAsync = true;
            base.Start();
            introLabel = root.Q<Label>(introTextLabel);
            progressBar = root.Q<ProgressBar>();
            introLabel.text = introText;
            startButton.visible = false;
            StartCoroutine(UpdateProgressBar());
        }

        protected override void StartButtonClicked()
        {
            AudioController.instance.PlayButtonSound(ButtonTypes.Start);
            SceneLoader.AllowSceneLoad(true);
        }

        private IEnumerator UpdateProgressBar()
        {
            progressBar.title = "Loading...";
            while (!SceneLoader.isSceneLoaded)
            {
                progressBar.value = SceneLoader.sceneProgress;
                yield return new WaitForEndOfFrame();
            }
            progressBar.title = "Ready";
            startButton.visible = true;
        }
    }
}