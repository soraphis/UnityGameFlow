using GameFlow.Core;
using Unity.Collections;
using UnityEngine;

[NodeTitle("Timer")]
public class FlowNode_Timer : GameFlowNodeBase
{
    [SerializeField, ReadOnly] private float runtime;
    [SerializeField, ReadOnly] private int stepCount;

    
    public float CompletionTime = 3.01f; 
    public float StepTime = 1.0f; 
    
    public override void Construct()
    {
#if UNITY_EDITOR
        nodeStyle = NodeStyle.Latent;
#endif
        inputPins.Clear();
        inputPins.Add("Start");
        inputPins.Add("Reset");
        
        outputPins.Clear();
        outputPins.Add("Completed");
        outputPins.Add("Step");
        
    }

    public override void ExecuteInput(string pinName)
    {
        switch (pinName)
        {
            case "Start":
                runtime = 0;
                stepCount = 1;
                GameFlowSubsystem.Instance.OnUpdateEvent -= TimerUpdate;
                GameFlowSubsystem.Instance.OnUpdateEvent += TimerUpdate;
                break;
            case "Reset":
                runtime = 0;
                stepCount = 1;
                GameFlowSubsystem.Instance.OnUpdateEvent -= TimerUpdate;
                break;    
        }
    }
    
    private void TimerUpdate(float deltaTime)
    {
        runtime += deltaTime;
        if (runtime >= (StepTime * stepCount))
        {
            stepCount++;
            TriggerOutput("Step", false);
        }
        if (runtime >= CompletionTime)
        {
            TriggerFirstOutput(true);
        }
    }

    public override void Cleanup()
    {
        runtime = 0;
        if(GameFlowSubsystem.Instance != null) GameFlowSubsystem.Instance.OnUpdateEvent -= TimerUpdate;
    }
}