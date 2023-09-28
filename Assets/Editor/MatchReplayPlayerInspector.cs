using System;
using System.Collections.Generic;
using System.Threading;
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

    private void OnSceneGUI()
    {
        TryPaintScene(null);
        PaintUI();
    }
    
    private void TryPaintScene(SceneView obj)
    {
        if (displayLevel && matchData != null)
        {
            var maxReplayTime = matchData.MatchDuration;

            foreach (var matchAgentData in matchData.AgentsData)
            {
                if(matchAgentData.Mute) continue;
                
                // Draw Position Events
                Handles.color = Color.green;
                var skippedFirstPositionEvent = false;
                for (var index = 0; index < matchAgentData.PositionEvents.Count; index++)
                {
                    var positionEvent = matchAgentData.PositionEvents[index];
                    
                    if(!TimeStampInsideRange(maxReplayTime, positionEvent.TimeStamp)) continue;

                    if (!skippedFirstPositionEvent)
                    {
                        skippedFirstPositionEvent = true;
                        continue;
                    }
                    
                    var previousPositionEvent = matchAgentData.PositionEvents[index - 1];
                    
                    if (matchAgentData.EliminationEvents.Exists(ee => 
                            previousPositionEvent.TimeStamp < ee.TimeStamp &&
                            positionEvent.TimeStamp > ee.TimeStamp)) continue;
                    
                        
                    Handles.DrawLine(previousPositionEvent.Pos / matchData.DecimalPrecision,
                        positionEvent.Pos / matchData.DecimalPrecision, 3.0f);
                }

                // Draw Elimination Events
                Handles.color = Color.red;
                for (var index = 0; index < matchAgentData.EliminationEvents.Count; index++)
                {
                    var eliminationEvent = matchAgentData.EliminationEvents[index];
                    
                    if(!TimeStampInsideRange(maxReplayTime, eliminationEvent.TimeStamp)) continue;
                    
                    Handles.DrawWireCube(eliminationEvent.Pos / matchData.DecimalPrecision, new Vector3(0.2f, 1f, 0.2f));
                    Handles.DrawWireCube(eliminationEvent.Pos / matchData.DecimalPrecision + new Vector3(0f, 0.2f, 0f), new Vector3(0.6f, 0.2f, 0.2f));
                }
            }
            SceneView.RepaintAll();
        }
    }

    private void PaintUI()
    {
        if (displayLevel && matchData != null)
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(20, 20, 100, 100));

            var rect = EditorGUILayout.BeginVertical();
            
            GUI.Box(rect, GUIContent.none);

            GUILayout.Label("Agents");

            EditorGUILayout.BeginVertical();
            
            foreach (var matchAgentData in matchData.AgentsData)
            {
                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Label(matchAgentData.AgentName);
                matchAgentData.Mute = GUILayout.Toggle(matchAgentData.Mute, "mute");

                var solo = false;
                solo = GUILayout.Toggle(solo, "solo");

                if (solo)
                {
                    matchAgentData.Mute = false;
                    foreach (var mAD in matchData.AgentsData)
                    {
                        if (mAD != matchAgentData)
                        {
                            mAD.Mute = true;
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }

    private bool TimeStampInsideRange(float matchTime, float timeStamp)
    {
        if (timeStamp / matchData.DecimalPrecision <
            matchTime * matchReplayPlayer.replayStartPercentage * 0.01f ||
            timeStamp / matchData.DecimalPrecision >
            matchTime * matchReplayPlayer.replayEndPercentage * 0.01f)
            return false;
        return true;
    }
}