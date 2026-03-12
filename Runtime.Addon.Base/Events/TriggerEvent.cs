using EventBus;
using UnityEngine;

#if EVENTBUS
public struct OnTriggerEnterEvent : IEvent
{
    public GameObject triggeringObject;
    public Collider   triggerCollider;
}

public struct OnTriggerExitEvent : IEvent
{
    public GameObject triggeringObject;
    public Collider   triggerCollider;
}
#endif