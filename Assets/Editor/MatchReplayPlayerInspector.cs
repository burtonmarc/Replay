using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting.YamlDotNet.Serialization.EventEmitters;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// TODO: Change how Agents are muted so they use the same system as muted MatchEventTypes
// TODO: Add option to replay multiple matches with same scene at the same time


[ExecuteInEditMode, CustomEditor(typeof(MatchReplayPlayer))]
public class MatchReplayPlayerInspector : UnityEditor.Editor
{
    private MatchReplayPlayer matchReplayPlayer;
    private static bool displayLevel;
    private static MatchData matchData;
    private static List<MatchEventType> eventTypesMuted;

    private void Awake()
    {
        matchReplayPlayer = target as MatchReplayPlayer;

        if (eventTypesMuted == null)
        {
            eventTypesMuted = new List<MatchEventType>();
        }
        
        SceneView.beforeSceneGui += TryDrawScene;
    }

    private void OnDestroy()
    {
        SceneView.beforeSceneGui -= TryDrawScene;
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
        TryDrawScene(null);
        DrawUI();
    }
    
    private void TryDrawScene(SceneView obj)
    {
        if (displayLevel && matchData != null)
        {
            var maxReplayTime = matchData.MatchDuration;

            foreach (var matchAgentData in matchData.AgentsData)
            {
                if(matchAgentData.Mute) continue;
                
                DrawPositionEvents(matchAgentData, maxReplayTime);

                DrawEliminationEvents(matchAgentData, maxReplayTime);
            }
            SceneView.RepaintAll();
        }
    }

    private void DrawPositionEvents(AgentData matchAgentData, float maxReplayTime)
    {
        if (eventTypesMuted != null && eventTypesMuted.Contains(MatchEventType.Position)) return;
        
        // Draw Position Events
        Handles.color = Color.green;
        var skippedFirstPositionEvent = false;
        for (var index = 0; index < matchAgentData.PositionEvents.Count; index++)
        {
            var positionEvent = matchAgentData.PositionEvents[index];

            if (!TimeStampInsideRange(maxReplayTime, positionEvent.TimeStamp)) continue;

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
    }
    
    private void DrawEliminationEvents(AgentData matchAgentData, float maxReplayTime)
    {
        if (eventTypesMuted != null && eventTypesMuted.Contains(MatchEventType.Elimination)) return;
        
        // Draw Elimination Events
        Handles.color = Color.red;
        for (var index = 0; index < matchAgentData.EliminationEvents.Count; index++)
        {
            var eliminationEvent = matchAgentData.EliminationEvents[index];

            if (!TimeStampInsideRange(maxReplayTime, eliminationEvent.TimeStamp)) continue;

            Handles.DrawWireCube(eliminationEvent.Pos / matchData.DecimalPrecision, new Vector3(0.2f, 1f, 0.2f));
            Handles.DrawWireCube(eliminationEvent.Pos / matchData.DecimalPrecision + new Vector3(0f, 0.2f, 0f),
                new Vector3(0.6f, 0.2f, 0.2f));
        }
    }

    private void DrawUI()
    {
        if (displayLevel && matchData != null)
        {
            Handles.BeginGUI();
            
            
            GUILayout.BeginArea(new Rect(20, 20, 200, 500));
            var rect = EditorGUILayout.BeginVertical();
            GUI.Box(rect, GUIContent.none);
            
            DrawAgentsUI();
            DrawEventsUI();
            DrawClearFiltersUI();
            
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            
            Handles.EndGUI();
        }
    }

    private void DrawAgentsUI()
    {
        GUILayout.Label("Agents");

        foreach (var matchAgentData in matchData.AgentsData)
        {
            EditorGUILayout.BeginHorizontal();
                
            GUILayout.Label(matchAgentData.AgentName);
            matchAgentData.Mute = GUILayout.Toggle(matchAgentData.Mute, "MUTE");

            var solo = GUILayout.Button("SOLO");

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

        Separator();
    }

    private void DrawEventsUI()
    {
        GUILayout.Label("Event Types");

        var matchEventTypes = Enum.GetNames (typeof(MatchEventType));

        if (eventTypesMuted == null)
        {
            eventTypesMuted = new List<MatchEventType>();
        }

        for (var index = 0; index < matchEventTypes.Length; index++)
        {
            EditorGUILayout.BeginHorizontal();
            
            var matchEventType = matchEventTypes[index];
            GUILayout.Label(matchEventType);

            var muted = GUILayout.Toggle(eventTypesMuted.Exists(eventType => eventType.ToString() == matchEventType), "MUTE");

            if (muted)
            {
                if (!eventTypesMuted.Contains((MatchEventType)index))
                {
                    eventTypesMuted.Add((MatchEventType)index);
                }
            }
            else
            {
                if (eventTypesMuted.Contains((MatchEventType)index))
                {
                    eventTypesMuted.Remove((MatchEventType)index);
                }
            }
            
            var solo = GUILayout.Button("SOLO");

            if (solo)
            {
                eventTypesMuted.Clear();
                foreach (var met in matchEventTypes)
                {
                    if (met != matchEventType)
                    {
                        var type = (MatchEventType)Enum.Parse( typeof(MatchEventType), met );
                        eventTypesMuted.Add(type);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        
        Separator();
    }

    private void DrawClearFiltersUI()
    {
        if (GUILayout.Button("CLEAR FILTERS"))
        {
            foreach (var mAD in matchData.AgentsData)
            {
                mAD.Mute = false;
            }
            
            eventTypesMuted.Clear();
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

    private void Separator()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("----------------");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}