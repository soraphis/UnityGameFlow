using System;
using UnityEngine;

namespace Runtime.Addon.Base.EventHandlers
{
    [RequireComponent( typeof(Collider))]
    public class OnTriggerEventBroadcast : MonoBehaviour
    {
        public bool RaiseEventOnEnter = true;
        public bool RaiseEventOnExit = false;

        private Collider selfCollider;
        
        private void Start()
        {
            selfCollider = this.GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
#if EVENTBUS
            if(RaiseEventOnEnter)
                EventBus.EventBus<OnTriggerEnterEvent>.Raise(new OnTriggerEnterEvent
                {
                    triggeringObject = other.gameObject,
                    triggerCollider  = selfCollider,
                });
#endif
        }        
        
        private void OnTriggerExit(Collider other)
        {
#if EVENTBUS
            if(RaiseEventOnExit)
                EventBus.EventBus<OnTriggerExitEvent>.Raise(new OnTriggerExitEvent
                {
                    triggeringObject = other.gameObject,
                    triggerCollider  = selfCollider,
                });
#endif
        }
        
    }
}