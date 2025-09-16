using System;
using System.Collections.Generic;
using GameFlow.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GameFlowEditorNodeView : Node
{
    private readonly GameFlowGraphAsset graphViewGraph;
    public GraphViewNode node;
    public Action<GameFlowEditorNodeView> OnNodeSelected;

    public Dictionary<string, Port> inputPorts = new();
    public Dictionary<string, Port> outputPorts = new();

    VisualElement descriptionContainer;
    
    private Color GetTypeColor(GraphViewNode node, Color fallback)
    {
        switch (node.NodeStyle)
        {
            case NodeStyle.None: return new Color(0, 0, 0);
            case NodeStyle.Default: return new Color(-0.728f, 0.581f, 1.0f);
            case NodeStyle.Control: return new Color(0.36f, 0.36f, 0.36f, 1f);
            case NodeStyle.Event: return new Color(1.0f, 0.62f, 0.016f);
            case NodeStyle.InOut: return new Color(1.0f, 0.0f, 0.008f);
            case NodeStyle.Latent: return new Color(0.0f, 0.770f, 0.375f);
            case NodeStyle.SubGraph: return new Color(1.0f, 0.128f, 0.0f);
            case NodeStyle.Custom: break;
        }
        return fallback;
    }
    
    public GameFlowEditorNodeView(GraphViewNode flowNode, GameFlowGraphAsset graphViewGraph,
        GameFlowGraphRunner activeFlow)
    {
        this.graphViewGraph = graphViewGraph;
        var titleColor = this.titleContainer.style.backgroundColor.value;
        titleColor.a = 0.95f;
        this.titleContainer.style.backgroundColor = Color.Lerp(GetTypeColor(flowNode, titleColor), titleColor, 0.5f);
        
        descriptionContainer = new VisualElement();
        descriptionContainer.AddToClassList("node-description");
        
        this.contentContainer.Q<VisualElement>("contents")?.Insert(0, descriptionContainer);
        
        var runtimeBorder = new VisualElement() { name = "runtime-border", pickingMode = PickingMode.Ignore};
        this.Add(runtimeBorder);

        var state = activeFlow?.GetNodeState(flowNode.guid) ?? FlowNodeRuntimeState.None;
        switch (state)
        {
            case FlowNodeRuntimeState.None: break;
            case FlowNodeRuntimeState.Started: this.AddToClassList("flowstate-active"); break;
            case FlowNodeRuntimeState.Finished: this.AddToClassList("flowstate-finished"); break;
            case FlowNodeRuntimeState.Aborted: break;
        }
        UpdateNode(flowNode);
    }
    
    public void UpdateNode(GraphViewNode flowNode)
    {
        this.node = flowNode;
        this.title = flowNode.title;
        this.viewDataKey = flowNode.guid;

        style.left = flowNode.position.x;
        style.top = flowNode.position.y;
        style.minWidth = 150;

        if (! string.IsNullOrWhiteSpace(flowNode.description))
        {
            descriptionContainer.Clear();
            descriptionContainer.style.display = DisplayStyle.Flex;
            descriptionContainer.Add(new Label(flowNode.description));
        }
        else
        {
            descriptionContainer.style.display = DisplayStyle.None;
        }

        CreateInputPorts();
        CreateOutputPorts();
    }

    private void CreatePorts(IReadOnlyList<string> data, VisualElement container, Dictionary<string, Port> ports, Direction portDirection, Port.Capacity capacity)
    {
        HashSet<string> previousKeys = new (ports.Keys);
        
        foreach (var portName in data)
        {
            previousKeys.Remove(portName);
            if(ports.ContainsKey(portName)) continue;
            
            Port p = InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(Control));
            p.portName = portName;
            container.Add(p);
            ports.Add(portName, p);
        }
        
        foreach (var key in previousKeys)
        {
            var port = ports[key];
            ports.Remove(key);
            container.Remove(port);
        }
    }
    private void CreateInputPorts() => CreatePorts(node.inputs, inputContainer, inputPorts, Direction.Input, Port.Capacity.Multi);
    private void CreateOutputPorts() => CreatePorts(node.outputs, outputContainer, outputPorts, Direction.Output, Port.Capacity.Single);


    public override void OnSelected()
    {
        base.OnSelected();
        OnNodeSelected?.Invoke(this);
    }


    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        Undo.RecordObject(graphViewGraph, "FlowGraph: Move Node");
        node.position = new Vector2(newPos.xMin, newPos.yMin);
        EditorUtility.SetDirty(graphViewGraph);

    }


}