
[NodeTitle("Start")]
public sealed class FlowNode_Start : GameFlowNodeBase
{
    public override bool CanUserAddInput() { return true; }
    public override void Construct()
    {
#if UNITY_EDITOR
        nodeStyle = NodeStyle.InOut;
#endif
        inputPins.Clear();
    }

    public override void ExecuteInput(string pinName)
    {
        TriggerFirstOutput(true);
    }
}