using System.Linq;
using GameFlow.Helper;
using UnityEngine;

[NodeTitle("MultiGate")]
public class FlowNode_MultiGate : GameFlowNodeBase
{
    // -- STATE (save-game-data) -------------------
    [SerializeField] private int nextGate = 0;
    
    // -- CONFIG -------------------
    public MultiGateMode mode = MultiGateMode.Once;


    public override bool CanUserAddOutput() => true;

    public override void Construct()
    {
#if UNITY_EDITOR
        nodeStyle = NodeStyle.Control;
#endif
        inputPins.Clear();
        inputPins.Add("In");
        inputPins.Add("Reset");
        
        outputPins = Enumerable.Range(0, 2).Select(x => x.ToString()).ToList();
    }

    public override void ExecuteInput(string pinName)
    {
        switch (pinName)
        {
            case "In":
                switch (mode)
                {
                    case MultiGateMode.Once:
                        if (nextGate >= outputPins.Count) return; // do nothing
                        TriggerOutput(nextGate, nextGate == outputPins.LastIndex());
                        nextGate++;
                        break;
                    case MultiGateMode.Loop:
                        nextGate = nextGate % outputPins.Count;
                        TriggerOutput(nextGate, false);
                        nextGate++;
                        break;
                    case MultiGateMode.RepeatLast:
                        TriggerOutput(Mathf.Min(nextGate, outputPins.LastIndex()), false);
                        break;
                }
                break;
            case "Reset":
                nextGate = 0;
                break;
        }
    }

    public override void Cleanup()
    {
    }
}