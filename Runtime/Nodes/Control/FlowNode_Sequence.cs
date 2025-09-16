using System.Linq;

[NodeTitle("Sequence")]
public class FlowNode_Sequence : GameFlowNodeBase
{
    public override bool CanUserAddOutput() { return true; }
    public override void Construct()
    {
#if UNITY_EDITOR
        nodeStyle = NodeStyle.Control;
#endif
        outputPins = Enumerable.Range(0, 3).Select(x => x.ToString()).ToList();
    }

    public override void ExecuteInput(string pinName)
    {
        for(int i = 0; i < outputPins.Count; i++)
        {
            TriggerOutput(i, false);
        }
    }
}