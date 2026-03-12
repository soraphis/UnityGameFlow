using System;
using UnityEngine;

namespace Runtime.Addon.Base.EventHandlers
{
    [RequireComponent( typeof(Collider))]
    public class OnTriggerEventBroadcast : MonoBehaviour
    {
        public bool RaiseEventOnEnter = true;
        public bool RaiseEventOnExit = false;
        
        private void OnTriggerEnter(Collider other)
        {
#if EVENTBUS
            if(RaiseEventOnEnter)
                EventBus.EventBus<OnTriggerEnterEvent>.Raise(new OnTriggerEnterEvent
                {
                    triggeringObject = gameObject,
                    triggerCollider  = other,
                });
            
            if(RaiseEventOnExit)
                EventBus.EventBus<OnTriggerExitEvent>.Raise(new OnTriggerExitEvent
                {
                    triggeringObject = gameObject,
                    triggerCollider  = other,
                });
#endif
        }
    }
}