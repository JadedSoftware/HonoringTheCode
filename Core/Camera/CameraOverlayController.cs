using System;
using Animancer.FSM;
using Core.Camera.OverlayStates;
using Core.GameManagement;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

namespace Core.Camera {
public enum CameraOverlayStates
{
    Movement, 
    Tactical,
    Hookshot,
    Ai,
    MeleeAttack,
    Grenade
}

    public class CameraOverlayController : MonoSingleton<CameraOverlayController>
    {
        public MovementOverlayState moveOverlayState;
        public TacticalOverlayState tacticalOverlayState;
        public HookshotOverlayState hookshotOverlayState;
        public AiOverlayState aiOverlayState;
        public MeleeAttackOverlayState meleeAttackOverlayState;
        public GrenadeOverlayState grenadeOverlayState;
        public CameraOverlayStates currentOverlayState => camOverlayStateMachine.CurrentKey;

        public StateMachine<CameraOverlayStates, CameraOverlayStateCommon> camOverlayStateMachine;

        public void OnCreate()
        {
            camOverlayStateMachine = new StateMachine<CameraOverlayStates, CameraOverlayStateCommon>();
            var pos = transform.position;
            moveOverlayState = new GameObject{
                transform =
                {
                    name = "MoveOverlayState",
                    parent = transform,
                    position = pos
                }
            }.AddComponent<MovementOverlayState>();     
            
            tacticalOverlayState = new GameObject{
                transform =
                {
                    name = "TacticalOverlayState",
                    parent = transform,
                    position = pos
                }
            }.AddComponent<TacticalOverlayState>();     
            
            hookshotOverlayState = new GameObject{
                transform =
                {
                    name = "hookshotOverlayState",
                    parent = transform,
                    position = pos
                }
            }.AddComponent<HookshotOverlayState>();            
            
            aiOverlayState = new GameObject{
                transform =
                {
                    name = "aiOverlayState",
                    parent = transform,
                    position = pos
                }
            }.AddComponent<AiOverlayState>();

            meleeAttackOverlayState = new GameObject
            {
                transform =
                {
                    name = "meleeAttackOverlayState",
                    parent = transform,
                    position = pos
                }
            }.AddComponent<MeleeAttackOverlayState>();

            grenadeOverlayState = new GameObject
            {
                transform =
                {
                    name = "grenadeOverlayState",
                    parent = transform,
                    position = pos
                }
            }.AddComponent<GrenadeOverlayState>();

            camOverlayStateMachine.Add(CameraOverlayStates.Movement, moveOverlayState);
            camOverlayStateMachine.Add(CameraOverlayStates.Tactical, tacticalOverlayState);
            camOverlayStateMachine.Add(CameraOverlayStates.Hookshot, hookshotOverlayState);
            camOverlayStateMachine.Add(CameraOverlayStates.Ai, aiOverlayState);
            camOverlayStateMachine.Add(CameraOverlayStates.MeleeAttack, meleeAttackOverlayState);
            camOverlayStateMachine.Add(CameraOverlayStates.Grenade, grenadeOverlayState);

            camOverlayStateMachine.ForceSetState(CameraOverlayStates.Movement);

            RegisterEvents();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }
        private void RegisterEvents()
        {
            EventSenderController.onOverlayChanged += OnOverlayChange;
        }
        private void UnRegisterEvents()
        {
            EventSenderController.onOverlayChanged -= OnOverlayChange;
        }

        private void OnOverlayChange(CameraOverlayStates overlayState)
        {
            camOverlayStateMachine.ForceSetState(overlayState);
        }

    }
    
}