using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameFlow.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

struct Control{}

public class GameFlowEditorGraphView : GraphView
{
    public new class UxmlFactory : UxmlFactory<GameFlowEditorGraphView, UxmlTraits>{ }

    // ---------------------------------------
    public Action<GameFlowEditorNodeView> OnNodeSelected;

    [SerializeField] private GameFlowGraphAsset graphViewGraph = null;
    [SerializeField] private GameFlowGraphRunner activeFlow;

    // ---------------------------------------
    public GameFlowEditorGraphView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        
        Undo.undoRedoPerformed += OnUndoRedo;
        
        var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.soraphis.gameflow/Editor/UI/GameFlowEditorWindow.uss"); // fixme: change to package path later :)
        styleSheets.Add(stylesheet);
    }

    private void OnUndoRedo()
    {
        PopulateView(graphViewGraph);
        AssetDatabase.SaveAssets();
    }

    public void UpdateActiveFlows(GameFlowGraphRunner activeFlow)
    {
        if(this.activeFlow == activeFlow) return;
        this.activeFlow = activeFlow;
        PopulateView(graphViewGraph);
    }
    
    public void PopulateView(GameFlowGraphAsset graphAsset)
    {
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        graphViewGraph = graphAsset;
        // create nodes
        foreach (var n in graphViewGraph.nodes) CreateNodeView(n);
        
        // create edges
        foreach (var connection in graphViewGraph.connections)
        {
            var outputNode = GetNodeByGuid(connection.outputGuid) as GameFlowEditorNodeView;
            var inputNode = GetNodeByGuid(connection.inputGuid) as GameFlowEditorNodeView;

            if (outputNode == null || inputNode == null)
            {
                Debug.LogError("GameFlowGraph: Faulty Connection");
                continue;
            }
            
            var edge = outputNode.outputPorts[connection.outputPin].ConnectTo(inputNode.inputPorts[connection.inputPin]);
            AddElement(edge);
        }
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphviewchange)
    {
        if (graphviewchange.elementsToRemove != null)
        {
            foreach (var elem in graphviewchange.elementsToRemove)
            {
                if(elem is GameFlowEditorNodeView nodeView) 
                    graphViewGraph.DeleteNode(nodeView.node);

                if (elem is Edge edge)
                {
                    if (edge.output.node is GameFlowEditorNodeView outputNode
                        && edge.input.node is GameFlowEditorNodeView inputNode)
                    {
                        graphViewGraph.RemoveConnection(outputNode.node, edge.output.portName, inputNode.node, edge.input.portName);
                    }
                }
            }
        }

        if (graphviewchange.edgesToCreate != null)
        {
            foreach (var edge in graphviewchange.edgesToCreate)
            {
                if (edge.output.node is GameFlowEditorNodeView outputNode 
                    && edge.input.node is GameFlowEditorNodeView inputNode)
                {

                    graphViewGraph.AddConnection(outputNode.node, edge.output.portName, inputNode.node, edge.input.portName);
                }
            }
        }
        
        return graphviewchange;
    }

    private void CreateNodeView(GraphViewNode graphViewNode)
    {
        GameFlowEditorNodeView view = new(graphViewNode, graphViewGraph, activeFlow);
        view.OnNodeSelected += (v) => OnNodeSelected?.Invoke(v);
        AddElement(view);
    }
    
    public void UpdateNode(GameFlowNodeBase node)
    {
        var nodeView = GetNodeByGuid(node.Guid) as GameFlowEditorNodeView;
        var graphViewNode = graphViewGraph.nodes.FirstOrDefault(n => n.guid == node.Guid);
        if (graphViewNode.node != null && nodeView != null)
        {
            nodeView.UpdateNode(graphViewNode);
        }
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
        // return base.GetCompatiblePorts(startPort, nodeAdapter);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        if (graphViewGraph == null)
        {
            evt.menu.AppendAction("No Graph Selected", (a) => {});
            return;
        }
        
        var types = TypeCache.GetTypesDerivedFrom<GameFlowNodeBase>();
        if (types.Count < 1)
        {
            evt.menu.AppendAction("No Nodes available", (a) => {});

        }

        var pos = evt.mousePosition;
        foreach (var type in types)
        {
            if(type.IsAbstract) continue;
            if(type == typeof(FlowNode_Start)) continue; // auto generated.
            
            var actionName = type.GetCustomAttribute<NodeTitleAttribute>()?.Title ?? type.Name; 
            
            evt.menu.AppendAction(actionName, (a) =>
            {
                var result = graphViewGraph.AddNode(type, pos);
                if (result != null)
                {
                    CreateNodeView(result.Value);
                }
            });
        }
        
    }


    
}
