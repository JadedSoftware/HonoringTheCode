using Core.NonLevel_Scenes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.MainMenu
{
    public class MainMenuUi : MenuUiCommon
    {
        private VisualElement mainMenuOverlay;
        private VisualElement buttonGroup;
        private Button loadGameButton;
        private Button settingsButton;
        protected override void Start()
        {
            base.Start();
            buttonGroup = root.Q<VisualElement>(UiCommonStrings.buttonGroupOverlay);
            loadGameButton = root.Q<Button>(UiCommonStrings.loadGameButton);
            loadGameButton.clicked += LoadButtonPressed;
            settingsButton = root.Q<Button>(UiCommonStrings.settingsButton);
            settingsButton.clicked += SettingsButtonPressed;
        }
        
        private void LoadButtonPressed()
        {
            Debug.Log("Load Button Pressed");
        }
        private void SettingsButtonPressed()
        {
            Debug.Log("Settings Button Pressed");
        }
    }
}