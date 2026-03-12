using UnityEngine;

[NodeTitle("DebugLog", "Debug/DebugLog")]
public class FlowNode_DebugLog : GameFlowNodeBase
{
    // -- CONFIG ----------------
    public string logMessage = "Hello World!"; 

    public override void Construct()
    {
#if UNITY_EDITOR
        nodeStyle = NodeStyle.Default;
#endif
    }

    public override void ExecuteInput(string pinName)
    {
        Debug.Log(logMessage);
        TriggerFirstOutput(true);
    }

#if UNITY_EDITOR
    protected internal override string GetNodeDescription()
    {
        return "Log: " + logMessage;
    }
#endif
}