using UnityEngine;
using UnityEngine.Serialization;

[NodeTitle("Counter")]
public class FlowNode_Counter : GameFlowNodeBase
{
    // -- STATE (save-game-data) -------------------
    [SerializeField] private int count;
    
    // -- CONFIG -------------------
    public int goal = 10;

    
    public override void Construct()
    {
#if UNITY_EDITOR
        nodeStyle = NodeStyle.Control;
#endif
        inputPins.Clear();
        inputPins.Add("Increment");
        inputPins.Add("Decrement");
        
        outputPins.Clear();
        outputPins.Add("Step");
        outputPins.Add("Goal");
    }

    public override void ExecuteInput(string pinName)
    {
        switch (pinName)
        {
            case "Increment":
                count++;
                TriggerOutput("Step", false);
                if (count == goal) TriggerOutput("Goal", true);
                break;
            case "Decrement":
                count--;
                TriggerOutput("Step", false);
                if (count == goal) TriggerOutput("Goal", true);
                break;
        }
    }

    public override void Cleanup()
    {
        count = 0;
    }
}


public enum MultiGateMode
{
    Once, Loop, RepeatLast
}