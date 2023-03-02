using System;
using System.Collections.Generic;
using Core.GameManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

namespace Core.UI
{
    public class  GameUiController : MonoSingleton<GameUiController>
    {
        public GameObject attackUIPrefab;
        private Button meleeAttackButton;
        [HideInInspector]public GameObject attackUI;
        GameOverlayController gameOverlay => GameOverlayController.instance;

        CrossFadeCanvas crossFadeCanvas;
        [Range(1, 10)] public float crossFadeTime = 3;

        private void Awake()
        {
            if(attackUI == null) attackUI = Instantiate(attackUIPrefab,transform);
            meleeAttackButton = attackUI.transform.GetChild(0).transform.GetChild(0).GetComponent<Button>();

            meleeAttackButton.onClick.AddListener(SetMeleeAttackOverlayState);
            meleeAttackButton.onClick.AddListener(()=>ToggleAttackUI(false));
            ToggleAttackUI(false);
        }
        private void OnEnable()
        {
            if (crossFadeCanvas == null)
            {
                crossFadeCanvas = GetComponentInChildren<CrossFadeCanvas>();
            }
            RegisterEvents();
        }
        public void OnDisable()
        {
            UnRegisterEvents();
        }

        public void LateUpdate()
        {
            if (attackUI.gameObject.activeSelf)
            {
                gameOverlay.IsPointerOverUI = IsMouseOverUi;
            }
        }

        public void ToggleAttackUI(bool toggle)
        {
            attackUI.gameObject.SetActive(toggle);
        }

        public void PositionAttackUIAtTarget(Vector3 position)
        {
            attackUI.transform.position = position + new Vector3(0f, 1.6f, 0f);
        }

        private void SetMeleeAttackOverlayState()
        {
            gameOverlay.SetMeleeAttackOverlayState();
        }
        private void RegisterEvents()
        {
            EventSenderController.onNavReady += NavsReady;
        }
        private void UnRegisterEvents()
        {
            EventSenderController.onNavReady -= NavsReady;
        }

        private void NavsReady()
        {
            crossFadeCanvas.FadeIn();
        }

        public void FadeInComplete()
        {
            EventSenderController.SceneFadedIn();
        }

        public static bool IsMouseOverUi
        {
            get
            {
                if (EventSystem.current == null)
                {
                    return false;
                }
                RaycastResult lastRaycastResult = ((InputSystemUIInputModule)EventSystem.current.currentInputModule).GetLastRaycastResult(Mouse.current.deviceId);
                
                return lastRaycastResult.gameObject != null && lastRaycastResult.gameObject.layer.Equals(LayerMask.NameToLayer("UI"));
            }
        }
    }
}