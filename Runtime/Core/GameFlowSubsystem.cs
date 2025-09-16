using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace GameFlow.Core
{
    public class FlowTimerManager
    {
        public event Action<float> OnUpdateEvent;
        
        public FlowTimerManager()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var updateLoopIdx = Array.FindIndex(playerLoop.subSystemList, s => s.type == typeof(Update));
            var updateLoop = playerLoop.subSystemList[updateLoopIdx];
            
            Array.Resize(ref updateLoop.subSystemList, updateLoop.subSystemList.Length + 1);
            updateLoop.subSystemList[^1] = new PlayerLoopSystem
            {
                type = typeof(FlowTimerManager),
                updateDelegate = OnUpdate
            };

            playerLoop.subSystemList[updateLoopIdx] = updateLoop;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private void OnUpdate()
        {
            OnUpdateEvent?.Invoke(Time.deltaTime);
        }
    }
    
    public class GameFlowSubsystem
    {
        public static GameFlowSubsystem Instance { get; private set; }
        
        private List<GameFlowComponent> gameFlowComponents = new List<GameFlowComponent>();

        private Dictionary<GameFlowGraphRunner, MonoBehaviour> rootInstances = new(); // Make serializable

        public event Action<float> OnUpdateEvent;
        private FlowTimerManager timerManager;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Instance = new GameFlowSubsystem();
            Instance.timerManager = new FlowTimerManager();
            Instance.timerManager.OnUpdateEvent += Instance.OnUpdate; 
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
            #endif
        }

        private void OnUpdate(float DeltaTime)
        {
            OnUpdateEvent?.Invoke(DeltaTime);
        }

        private static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingPlayMode)
            {
                EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
                Instance.Dispose();
                Instance = null;
            }
        }

        public static void RegisterComponent(GameFlowComponent gameFlowComponent)
        {
            Instance.gameFlowComponents.Add(gameFlowComponent);
        }
        
        public static void UnregisterComponent(GameFlowComponent gameFlowComponent)
        {
            Instance.gameFlowComponents.Remove(gameFlowComponent);
        }

        public static void StartRootFlow(GameFlowComponent owner, GameFlowGraphAsset graphAsset)
        {
            var runner = new GameFlowGraphRunner();
            runner.Initialize(owner, graphAsset);
            Instance.rootInstances.Add(runner, owner);
            runner.StartFlow();
        }

        public void GetActiveFlowsForGraph(GameFlowGraphAsset selectedAsset, in List<GameFlowGraphRunner> runners)
        {
            foreach (var (runner, _) in rootInstances)
            {
                if (runner.GraphAsset == selectedAsset)
                {
                    runners.Add(runner);
                }
            }
        }

        private void Dispose()
        {
            foreach (var (runner, _) in rootInstances)
            {
                runner.Dispose();
            }
        }

    }
}