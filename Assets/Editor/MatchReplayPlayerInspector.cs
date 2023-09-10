using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode, CustomEditor(typeof(MatchReplayPlayer))]
public class MatchReplayPlayerInspector : UnityEditor.Editor
{
    private MatchReplayPlayer matchReplayPlayer;
    private bool displayLevel;
    private MatchData matchData;

    private void Awake()
    {
        matchReplayPlayer = target as MatchReplayPlayer;
        SceneView.beforeSceneGui += TryPaintScene;
    }

    private void OnDestroy()
    {
        SceneView.beforeSceneGui -= TryPaintScene;
    }

    private void OnSceneGUI()
    {
        TryPaintScene(null);
    }
    
    private void TryPaintScene(SceneView obj)
    {
        if (displayLevel && matchData != null)
        {
            Handles.color = Color.green;

            var maxReplayTime = matchData.MatchDuration;

            foreach (var matchAgentData in matchData.AgentsData)
            {
                for (var index = 0; index < matchAgentData.PositionEvents.Count; index++)
                {
                    var positionEvent = matchAgentData.PositionEvents[index];
                    if(index == 0) continue;
                    var previousPositionEvent = matchAgentData.PositionEvents[index - 1];
                    
                    Handles.DrawLine(previousPositionEvent.Pos, positionEvent.Pos);
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Load Match Replay Files"))
        {
            matchReplayPlayer.LoadMatchDataFiles();
        }
        base.OnInspectorGUI();

        GUILayout.BeginVertical("Start and End of the Replay", "window");
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Replay Start: {(int)matchReplayPlayer.replayStartPercentage}%");
        EditorGUILayout.LabelField($"Replay End: {(int)matchReplayPlayer.replayEndPercentage}%");
        GUILayout.EndHorizontal();
        EditorGUILayout.MinMaxSlider( ref matchReplayPlayer.replayStartPercentage, ref matchReplayPlayer.replayEndPercentage, 0f, 100f);
        GUILayout.EndVertical();
        
        for (var index = 0; index < matchReplayPlayer.allMatchesData.Count; index++)
        {
            // I will need the scene name to load it
            var matchSerializableData = matchReplayPlayer.allMatchesData[index];
            if (GUILayout.Button($"Replay Match {index}"))
            {
                displayLevel = true;
                matchData = new MatchData(matchSerializableData);
            }
        }
    }
    
/*
    private void CreateGUI()
    {
        // Reference to the root of the window.
        var root = rootVisualElement;

        // Associates a stylesheet to our root. Thanks to inheritance, all root’s
        // children will have access to it.
        root.styleSheets.Add(Resources.Load<StyleSheet>("QuickTool_Style"));
        
        // Loads and clones our VisualTree (eg. our UXML structure) inside the root.
        var quickToolVisualTree = Resources.Load<VisualTreeAsset>("QuickTool_Main");
        quickToolVisualTree.CloneTree(root);

        // Queries all the buttons (via class name) in our root and passes them
        // in the SetupButton method.
        var toolButtons = root.Query(className: "quicktool-button");
        toolButtons.ForEach(SetupButton);

        var createLineButton = root.Query(className: "create-line-button");
        SetupCreateLineButton(createLineButton);
    }
*/

    private void SetupCreateLineButton(VisualElement createLineButton)
    {
        createLineButton.RegisterCallback<PointerUpEvent, Vector3>(DrawLine, Vector3.zero);
    }

    private void DrawLine(PointerUpEvent _, Vector3 start)
    {
        Handles.DrawLine(Vector3.zero, Vector3.one * 4f);
    }

    private void SetupButton(VisualElement button) 
    {
        // Reference to the VisualElement inside the button that serves
        // as the button's icon.
        var buttonIcon = button.Q(className: "quicktool-button-icon");

        // Icon's path in our project.
        var iconPath = "Icons/" + button.parent.name + "_icon";

        // Loads the actual asset from the above path.
        var iconAsset = Resources.Load<Texture2D>(iconPath);

        // Applies the above asset as a background image for the icon.
        buttonIcon.style.backgroundImage = iconAsset;

        // Instantiates our primitive object on a left click.
        button.RegisterCallback<PointerUpEvent, string>(CreateObject, button.parent.name);

        // Sets a basic tooltip to the button itself.
        button.tooltip = button.parent.name;
    }
    
    private void CreateObject(PointerUpEvent _, string primitiveTypeName)
    {    
        var pt = (PrimitiveType) Enum.Parse
            (typeof(PrimitiveType), primitiveTypeName, true);
        var go = ObjectFactory.CreatePrimitive(pt);
        go.transform.position = Vector3.zero;
    }
}