using EventBus;
using Runtime.Addon.Base.ExpressionContext;

#if EXPRESSIONS
using UnityInspectorExpressions.Expressions;
#endif

namespace Runtime.Addon.Base.CustomNodes.Events
{
    [NodeTitle("AwaitTriggerExit", "Events/AwaitTriggerEnter")]
    public class FlowNode_AwaitTriggerEnter : GameFlowNodeBase
    {
        // -- CONFIG ----------------
#if EXPRESSIONS
        public BoolExpression<TriggerContext> Filter;
#endif
        
#if EVENTBUS
        private EventBinding<OnTriggerEnterEvent> _binding;
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
            _binding = EventBinding<OnTriggerEnterEvent>.Subscribe(HandleTriggerEnter);
#endif
        }

        public override void Cleanup()
        {
            base.Cleanup();
#if EVENTBUS
            _binding.Dispose();
#endif
        }

        private void HandleTriggerEnter(OnTriggerEnterEvent obj)
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