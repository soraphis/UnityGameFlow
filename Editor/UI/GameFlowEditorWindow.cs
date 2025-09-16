using System;
using System.Collections.Generic;
using System.Linq;
using GameFlow.Core;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

internal class DoCreateNewAsset : EndNameEditAction
{
    public Action m_Action;
    
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
        m_Action?.Invoke();
    }

    public override void Cancelled(int instanceId, string pathName, string resourceFile)
    {
        Selection.activeObject = (UnityEngine.Object) null;
    }
}


// https://www.youtube.com/watch?app=desktop&v=nKpM98I7PeM
public class GameFlowEditorWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    // -----------------
    [SerializeField] private GameFlowEditorGraphView graphView;
    [SerializeField] private GameFlowEditorInspectorView inspectorView;
    [SerializeField] private DropdownField activeObjectSwitcher;

    [SerializeField] private GameFlowGraphAsset selectedAsset;

    // -----------------
    
    
    [MenuItem("Assets/Create/GameFlowGraphAsset")]
    public static void CreateCustomAssetWithChild()
    {
        // Determine the selected folder path in the Project window or default to "Assets"
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }
        else if (!System.IO.Directory.Exists(path))
        {
            path = System.IO.Path.GetDirectoryName(path);
        }

        // Construct the full path for the main asset
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/New GameFlowGraphAsset.asset");
        GameFlowGraphAsset mainAsset = ScriptableObject.CreateInstance<GameFlowGraphAsset>();

        var createAssetCallback = ScriptableObject.CreateInstance<DoCreateNewAsset>();
        createAssetCallback.m_Action = () =>
        {
            mainAsset.Construct();
            Selection.activeObject = mainAsset;
        };
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(mainAsset.GetInstanceID(), createAssetCallback, assetPath, AssetPreview.GetMiniThumbnail(mainAsset), (string) null);
    }
    
    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        labelFromUXML.style.flexGrow = 1;
        root.Add(labelFromUXML);
        
        var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/GameFlow/Editor/UI/GameFlowEditorWindow.uss"); // fixme: change to package path later :)
        root.styleSheets.Add(stylesheet);
        
        graphView = root.Q<GameFlowEditorGraphView>();
        inspectorView = root.Q<GameFlowEditorInspectorView>();
        activeObjectSwitcher = root.Q<DropdownField>("ActiveObjectSwitcher");
        activeObjectSwitcher.choices = new();
        graphView.OnNodeSelected = (node) => inspectorView.PopulateView(node);
        inspectorView.OnNodeChanged = (node) => graphView.UpdateNode(node);
        
        if(selectedAsset != null) graphView.PopulateView(selectedAsset);
    }


    private void OnEnable()
    {
        EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
        EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
    }

    private void EditorApplicationOnplayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // TODO: make graph readonly
            UpdateActiveFlows();
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            
        }
    }

    private void UpdateActiveFlows()
    {
        if(activeObjectSwitcher == null) return;
        if (GameFlowSubsystem.Instance == null)
        {
            activeObjectSwitcher.choices.Clear();
            activeObjectSwitcher.visible = false;
            return;
        }

        List<GameFlowGraphRunner> activeFlows = new();
        GameFlowSubsystem.Instance.GetActiveFlowsForGraph(selectedAsset, activeFlows);

        var newChoices = activeFlows.Select(x => x.Owner.gameObject?.name).ToList();
        if(activeObjectSwitcher.choices.SequenceEqual(newChoices)) return;

        if (activeObjectSwitcher.index != -1)
        {
            var currentChoice = activeObjectSwitcher.value;
            activeObjectSwitcher.index = newChoices.IndexOf(currentChoice);
        }
        activeObjectSwitcher.choices = newChoices.ToList();
        if (activeObjectSwitcher.index == -1) activeObjectSwitcher.index = 0;
        
        graphView.UpdateActiveFlows(activeFlows[activeObjectSwitcher.index]);
    }

    private void OnFocus()
    {
        UpdateActiveFlows();
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
    }

    [OnOpenAsset]
    public static bool OpenAsset(int instanceID, int line)
    {
        string assetPath = AssetDatabase.GetAssetPath(instanceID);
        var asset = AssetDatabase.LoadAssetAtPath<GameFlowGraphAsset>(assetPath);
        if (asset == null) return false;

        var openWindow = Resources.FindObjectsOfTypeAll<GameFlowEditorWindow>()
            .Where(x => x.selectedAsset == asset);

        GameFlowEditorWindow window;
        window = openWindow.FirstOrDefault() ?? EditorWindow.CreateWindow<GameFlowEditorWindow>("FlowGraph: " + asset.name);
        window.Focus();
        window.SelectAsset(asset);
        return true;
    }

    private void SelectAsset(GameFlowGraphAsset asset)
    {
        selectedAsset = asset;
        graphView.PopulateView(selectedAsset);
    }


    private void OnSelectionChange()
    {
        // if (Selection.activeObject is GameFlowGraphAsset graphAsset) // TODO: replace type by actual type :D
        // {
        //     graphView.PopulateView(graphAsset);
        // }
    }
}
