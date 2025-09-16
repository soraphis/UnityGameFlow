using System;
using System.Collections.Generic;
using System.Reflection;
using GameFlow.Core;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

public enum NodeStyle
{
    None = 0, Default = 1, Control = 2, Event = 3, InOut = 4, 
    Latent = 5, SubGraph = 6, 
    
    Custom = 16
}

public interface IGameFlowLifecycle
{
    // public void InitializeInstance();
    // public void PreloadContent();
    // public void OnActivate();
    /// <summary>Called in the Editor when the Node is initialized newly</summary>
    public void ExecuteInput(string pinName);
    // public void ForceFinishNode();
    public void Cleanup();
    // public void FlushContent();
    // public void DeinitializeInstance();
}

[System.Serializable]
public abstract class GameFlowNodeBase : IGameFlowLifecycle // TODO: serializationcallbackreceiver to cleanup duplicates in Input/Output pins
{
    //- serializable properties -----------------------------------
    [SerializeField, HideInInspector] protected List<string> inputPins = new (){""};
    [SerializeField, HideInInspector] protected List<string> outputPins = new (){"Out"};
    [SerializeField, ReadOnly] protected string guid;
#if UNITY_EDITOR
    [SerializeField, HideInInspector] protected NodeStyle nodeStyle = NodeStyle.Default;
    // [SerializeField] protected short customNodeStyle = -1; TODO
    // [SerializeField] protected string Category = "";
    [SerializeField, HideInInspector] public Vector2 position;
#endif

    private IFlowRunner runner;
    
    //- public getters ----------------------------------- 
    public virtual bool CanUserAddInput() => false;
    public virtual bool CanUserAddOutput() => false;
    public IReadOnlyList<string> GetInputs() => inputPins;
    public IReadOnlyList<string> GetOutputs() => outputPins;
    public virtual string Title => this.GetType().GetCustomAttribute<NodeTitleAttribute>()?.Title ?? this.GetType().Name;
    public NodeStyle NodeStyle => nodeStyle;
    public string Guid
    {
        get => guid;
        internal set => guid = value;
    }

    public GameFlowNodeBase Clone()
    {
        string json = JsonUtility.ToJson(this);
        return (GameFlowNodeBase)JsonUtility.FromJson(json, this.GetType());
    }
    

    //- Lifecycle events -----------------------------------
    
    /// <summary>Called in the Editor when the Node is constructed</summary>
    public virtual void Construct() {}

    public virtual void Cleanup() {}
    
    public virtual void ExecuteInput(string pinName){ TriggerFirstOutput(true); } // TODO: make protected

    //- Output events -----------------------------------
    
    public void TriggerInput(IFlowRunner runner, string pinName)
    {
        this.runner = runner;
        ExecuteInput(pinName);
    }
    
    internal void TriggerFirstOutput(bool finished) => TriggerOutput(0, finished);
    protected void TriggerOutput(int i, bool finished) => TriggerOutput(outputPins[i], finished);
    
    protected void TriggerOutput(string pinName, bool finished)
    {
        // TODO: trigger output.
        if (finished) Cleanup();
        
        runner.TriggerOutput(this.guid, pinName, finished);
    }

#if UNITY_EDITOR
    internal virtual string GetNodeDescription()
    {
        return "";
    }
#endif
    
    //- Factory Functions -----------------------------------

#if UNITY_EDITOR
    internal static GameFlowNodeBase CreateNode(System.Type type, ScriptableObject parent)
    {
        if (!typeof(GameFlowNodeBase).IsAssignableFrom(type)) return null;
        if (parent == null) return null;

        var so = Activator.CreateInstance(type); // ScriptableObject.CreateInstance(type);
        if (so is GameFlowNodeBase n)
        {
            n.Guid = GUID.Generate().ToString();
            n.Construct();
            // AssetDatabase.AddObjectToAsset(n, parent);
            return n;
        }
        // else if(so != null) ScriptableObject.Destroy(so);
        return null;
    }
    
    internal static GameFlowNodeBase CreateNode<T>(ScriptableObject parent)
        where T : GameFlowNodeBase
    {
        return CreateNode(typeof(T), parent);
    }
    
#endif


}