using System;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

internal class NodeWrapper : ScriptableObject
{
    [SerializeReference] public GameFlowNodeBase NodeBase;
    
    public Action<GameFlowNodeBase> OnNodeChanged;
    public void TriggerOnNodeChanged()
    {
        OnNodeChanged?.Invoke(NodeBase);
    }
}

// Custom Inspector for NodeWrapper:
[CustomEditor(typeof(NodeWrapper))]
public class NodeWrapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        try{
            serializedObject.Update();
            var mainProperty = serializedObject.FindProperty("NodeBase");
            
            var outputsProperty = mainProperty.FindPropertyRelative("outputPins");
            IncreasePinCount(outputsProperty, ((NodeWrapper)target).NodeBase.CanUserAddOutput(), "Add Output Pin");
            var inputsProperty = mainProperty.FindPropertyRelative("inputPins");
            IncreasePinCount(inputsProperty, ((NodeWrapper)target).NodeBase.CanUserAddInput(), "Add Input Pin");
            
            
            mainProperty.NextVisible(true);
            do
            {
                var inner = mainProperty.Copy();
                var attribs = EditorUtils.GetAttributes<ReadOnlyAttribute>(inner, true);
                EditorGUI.BeginDisabledGroup(attribs.Length > 0); 
                EditorGUILayout.PropertyField(mainProperty, true);
                EditorGUI.EndDisabledGroup();
            } while (mainProperty.NextVisible(false));

            
            if (serializedObject.ApplyModifiedProperties())
            {
                ((NodeWrapper)target)?.TriggerOnNodeChanged();
            }
            
            
        } catch (System.Exception e) {
            EditorGUILayout.HelpBox(e.Message, MessageType.Error);
            EditorGUILayout.TextArea(e.StackTrace);
        }
    }

    private void IncreasePinCount(SerializedProperty pinProperty, bool condition, string buttonTitle)
    {
        if (condition)
        {
            if(GUILayout.Button(buttonTitle))
            {
                // TODO: count numbered outputs and append one.
                pinProperty.InsertArrayElementAtIndex(pinProperty.arraySize);
                pinProperty.GetArrayElementAtIndex(pinProperty.arraySize - 1).stringValue = (pinProperty.arraySize-1).ToString();
            }
        }
    }
}

public class GameFlowEditorInspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<GameFlowEditorInspectorView, UxmlTraits>
    {
    }

    [SerializeField] private Editor editor;
    [SerializeField] NodeWrapper wrapper;
    
    public Action<GameFlowNodeBase> OnNodeChanged;
    public void TriggerOnNodeChanged(GameFlowNodeBase nodeBase)
    {
        OnNodeChanged?.Invoke(nodeBase);
    }
    
    public GameFlowEditorInspectorView()
    {
        wrapper = new NodeWrapper();
        wrapper.OnNodeChanged += TriggerOnNodeChanged;
        
        var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.soraphis.gameflow/Editor/UI/GameFlowEditorWindow.uss"); // fixme: change to package path later :)
        
        styleSheets.Add(stylesheet);
    }
    
    
    
    public void PopulateView(GameFlowEditorNodeView node)
    {
        Clear();
        var actualNode = node.node.node;
        wrapper.NodeBase = actualNode;

        if(editor != null) UnityEngine.Object.DestroyImmediate(editor);
        editor = Editor.CreateEditor(wrapper);
        IMGUIContainer container = new IMGUIContainer(() =>
        {
            if(editor && editor.target) editor.OnInspectorGUI();
        });
        Add(container);
        
    }
}