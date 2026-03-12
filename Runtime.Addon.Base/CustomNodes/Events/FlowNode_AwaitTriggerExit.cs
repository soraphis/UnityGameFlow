using EventBus;
using Runtime.Addon.Base.ExpressionContext;
using UnityInspectorExpressions.Expressions;

namespace Runtime.Addon.Base.CustomNodes.Events
{
    [NodeTitle("AwaitTriggerExit", "Events/AwaitTriggerExit")]
    public class FlowNode_AwaitTriggerExit : GameFlowNodeBase
    {
        // -- CONFIG ----------------
#if EXPRESSIONS
        public BoolExpression<TriggerContext> Filter;
#endif
        
#if EVENTBUS
        private EventBinding<OnTriggerExitEvent> _binding;
#endif
        
        public override void Construct()
        {
#if UNITY_EDITOR
            nodeStyle = NodeStyle.Event;
#endif
            inputPins.Clear();
            inputPins.Add("Start Listening");
        
            outputPins.Clear();
            outputPins.Add("Triggered");
        }

        public override void ExecuteInput(string pinName)
        {
#if EVENTBUS
            _binding = EventBinding<OnTriggerExitEvent>.Subscribe(HandleTriggerEnter);
#endif
        }

        public override void Cleanup()
        {
            base.Cleanup();
#if EVENTBUS
            _binding.Dispose();
#endif
        }

        private void HandleTriggerEnter(OnTriggerExitEvent obj)
        {
#if EXPRESSIONS
            var result = Filter.Evaluate(new TriggerContext
            {
                TriggeringObject = obj.triggeringObject,
                TriggerCollider  = obj.triggerCollider,
            });
            if(! result) return;
#endif
            TriggerFirstOutput(true);
        }
    }
}