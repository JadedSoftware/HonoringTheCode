using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.GameManagement.EventSenders
{
    public static partial class EventSenderController
    {
        private static Queue<Action> eventQueue = new Queue<Action>();
        private static bool isInvokingEvents = false;

        private static void ScheduleEvent(Action action)
        {
            eventQueue.Enqueue(action);

            if (isInvokingEvents) return;
            isInvokingEvents = true;

            // Invoke Queued Events
            while (eventQueue.TryDequeue(out Action eventAction))
            {
                try {
                    eventAction?.Invoke();
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }

            isInvokingEvents = false;
        }
    }
}