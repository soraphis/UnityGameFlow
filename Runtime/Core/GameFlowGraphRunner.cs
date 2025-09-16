using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GameFlow.Core
{

    public interface IFlowRunner
    {
        public void TriggerOutput(string nodeGuid, string pinName, bool finished);
    }

    public enum FlowNodeRuntimeState
    {
        None = 0, Started = 1, Finished = 2, Aborted = 3
    }
    
    [System.Serializable]
    public class GameFlowGraphRunner : IFlowRunner
    {
        [SerializeField] private MonoBehaviour owner = null;
        [SerializeField] private GameFlowGraphAsset graphAsset = null;
        [SerializeField] private List<string> recordedNodes = new();
        [SerializeField] private List<string> finishedNodes = new(); // do I need this for more than debug reasons?

        public MonoBehaviour Owner => owner;
        public GameFlowGraphAsset GraphAsset => graphAsset;
        
        // --- runtime nodes data:
        [SerializeField] private List<GraphViewNode> _nodes = new();
        
        ~GameFlowGraphRunner(){ Dispose(); }
        public void Dispose()
        {
            foreach (var recordedNode in recordedNodes)
            {
                // TODO: force finish nodes:
                
            }
            recordedNodes.Clear();
            finishedNodes.Clear();
            _nodes.Clear();
        }
        
        public void Initialize(MonoBehaviour owner, GameFlowGraphAsset graphAsset)
        {
            this.owner = owner;
            this.graphAsset = graphAsset;
            graphAsset.GetNodeCopies(_nodes);
        }

        public void StartFlow()
        {
            recordedNodes.Add(graphAsset.startNode.guid);
            graphAsset.startNode.TriggerInput(this, "");
        }

        public void TriggerInput(GraphViewNode node, string pinName)
        {
            recordedNodes.Add(node.guid);
            node.node.TriggerInput(this, pinName);
        }

        void IFlowRunner.TriggerOutput(string nodeGuid, string pinName, bool finished)
        {
            var connection = graphAsset.connections.FirstOrDefault(c => c.outputGuid == nodeGuid && c.outputPin == pinName);
            if (string.IsNullOrEmpty(connection.inputGuid)) return;
            
            var targetGuid = connection.inputGuid;
            var inputNode = _nodes.First(n => n.guid == targetGuid);
            TriggerInput(inputNode, connection.inputPin);
            if(finished) finishedNodes.Add(nodeGuid);
        }

        public FlowNodeRuntimeState GetNodeState(string flowNodeGuid)
        {
            return recordedNodes.Contains(flowNodeGuid) ? FlowNodeRuntimeState.Started
                    : finishedNodes.Contains(flowNodeGuid) ? FlowNodeRuntimeState.Finished
                    : FlowNodeRuntimeState.None;
        }


    }
}