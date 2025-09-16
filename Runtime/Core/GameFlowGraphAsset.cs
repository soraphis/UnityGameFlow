using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFlow.Core;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[System.Serializable]
public struct Connection
{
    public string outputGuid;
    public string outputPin;
    public string inputGuid;
    public string inputPin;

    public bool Equals(Connection other)
    {
        return outputGuid == other.outputGuid && outputPin == other.outputPin && inputGuid == other.inputGuid && inputPin == other.inputPin;
    }

    public override bool Equals(object obj)
    {
        return obj is Connection other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(outputGuid, outputPin, inputGuid, inputPin);
    }
}

[System.Serializable]
public struct GraphViewNode
{
    [SerializeReference] public GameFlowNodeBase node;
    public string title => node.Title;
    public IReadOnlyList<string> inputs => node.GetInputs();
    public IReadOnlyList<string> outputs => node.GetOutputs();
    public Vector2 position { get => node.position; set => node.position = value; }
    public string guid => node.Guid;
    public NodeStyle NodeStyle => node.NodeStyle;
    public string description => node.GetNodeDescription();


    public void TriggerFirstOutput() => node.TriggerFirstOutput(true);
    public void TriggerInput(IFlowRunner runner, string pinName) => node.TriggerInput(runner, pinName);

    public GraphViewNode Clone()
    {
        return new()
        {
            node = node.Clone(),
        };
    }
}

public interface IGraphViewGraph
{
    IEnumerable<GraphViewNode> nodes { get; }
    List<Connection> connections { get; }
    bool DeleteNode(GraphViewNode node);
}



public class GameFlowGraphAsset : ScriptableObject, IGraphViewGraph
{
    // https://github.com/MothCocoon/FlowGraph

    [SerializeField] private GraphViewNode _startNode;
    [SerializeField] private List<GraphViewNode> _nodes = new();
    [SerializeField] private List<Connection> _connections = new();
    
    public GraphViewNode startNode => _startNode;
    public IEnumerable<GraphViewNode> nodes => _nodes.Prepend(_startNode).Cast<GraphViewNode>();
    public List<Connection> connections => _connections;


    internal void GetNodeCopies(List<GraphViewNode> outList)
    {
        outList.Clear();
        foreach (var graphViewNode in _nodes)
        {
            outList.Add(graphViewNode.Clone());
        }
    }
    
    public void AddConnection(GraphViewNode outputNodeNode, string outputPortName, GraphViewNode inputNodeNode, string inputPortName)
    {
        Undo.RecordObject(this, "FlowGraph: Add Connection");
        _connections.Add(new ()
        {
            outputGuid = outputNodeNode.guid,
            outputPin = outputPortName,
            inputGuid = inputNodeNode.guid,
            inputPin = inputPortName
        });
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public void RemoveConnection(GraphViewNode outputNodeNode, string outputPortName, GraphViewNode inputNodeNode, string inputPortName)
    {
        Undo.RecordObject(this, "FlowGraph: Remove Connection");
        Connection connection = new()
        {
            outputGuid = outputNodeNode.guid,
            outputPin = outputPortName,
            inputGuid = inputNodeNode.guid,
            inputPin = inputPortName
        };
        _connections.Remove(connection);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public bool DeleteNode(GraphViewNode node)
    {
        if (node.guid == _startNode.guid) return false;
        Undo.RecordObject(this, "FlowGraph: Delete Node");
        int count = 0;
        for (int i = _nodes.Count - 1; i >= 0; i--)
        {
            if (_nodes[i].guid == node.guid)
            {
                // AssetDatabase.RemoveObjectFromAsset(_nodes[i].node);
                // ScriptableObject.DestroyImmediate(_nodes[i].node);
                _nodes.RemoveAt(i);
                count++;
            }
        }
        if (count > 0)
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            return true;            
        }
        return false;
    }
    
    public GraphViewNode? AddNode(Type type, Vector2 evtMousePosition)
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "FlowGraph: Add Node");
        var n = GameFlowNodeBase.CreateNode(type, this);
        if (n != null)
        {
            var graphNode = new GraphViewNode() { node = n, position = evtMousePosition };
            _nodes.Add(graphNode);
            // n.name = n.Title + "_" + _nodes.Count;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            return graphNode;
        }
#endif
        return null;
    }


    public void Construct()
    {
        if (_startNode.node == null)
        {
#if UNITY_EDITOR
            var n = GameFlowNodeBase.CreateNode<FlowNode_Start>(this);
            if (n != null)
            {
                var graphNode = new GraphViewNode() { node = n };
                _startNode = graphNode;
                // n.name = n.Title;
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
#endif
        }
    }
}
