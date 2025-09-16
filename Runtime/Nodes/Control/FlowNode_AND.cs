using System.Collections.Generic;
using System.Linq;
using GameFlow.Helper;
using UnityEngine;

[NodeTitle("AND")]
public class FlowNode_AND : GameFlowNodeBase
{
    [SerializeField] private List<string> executedInputPins; // save-game-data

    public override bool CanUserAddInput() { return true; }
    
    public override void Construct()
    {
#if UNITY_EDITOR
        nodeStyle = NodeStyle.Control;
#endif
        inputPins = Enumerable.Range(0, 2).Select(x => x.ToString()).ToList();
    }

    public override void ExecuteInput(string pinName)
    {
        executedInputPins.AddUnique(pinName);
        if (executedInputPins.Count == inputPins.Count)
        {
            TriggerFirstOutput(true);
        }
    }

    public override void Cleanup()
    {
        executedInputPins.Clear();
    }
}