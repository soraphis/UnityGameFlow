using System;
using UnityEngine;

namespace GameFlow.Core
{
    public class GameFlowComponent : MonoBehaviour
    {
        [SerializeField] private GameFlowGraphAsset graphAsset;

        private void OnEnable()
        {
            GameFlowSubsystem.RegisterComponent(this);
            if (graphAsset == null) return;
            GameFlowSubsystem.StartRootFlow(this, graphAsset);
        }
    }
}