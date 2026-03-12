#if EXPRESSIONS
using Unity.Properties;
using UnityEngine;

namespace Runtime.Addon.Base.ExpressionContext
{
    [System.Serializable, GeneratePropertyBag]
    public struct TriggerContext
    {
        [CreateProperty] public GameObject TriggeringObject { get; set; }
        [CreateProperty] public Collider   TriggerCollider { get; set; }
    }
}
#endif