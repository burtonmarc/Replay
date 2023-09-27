using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchReplayRecorder : MonoBehaviour
{
    public static MatchReplayRecorder Instance;

    public static float MatchElapsedTime;
    
    private List<MatchReplayAgent> activeReplayAgents;
    private List<MatchReplayAgent> inactiveReplayAgents;
    private AgentData.AgentSerializableData[] agentsSerializableData;

    public const int DecimalPrecision = 100;

    void Awake()
    {
        activeReplayAgents = new List<MatchReplayAgent>(32);
        inactiveReplayAgents = new List<MatchReplayAgent>(32);
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        MatchElapsedTime += Time.deltaTime;
        
        foreach (var replayAgent in activeReplayAgents)
        {
            replayAgent.CustomUpdate();
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            FinishRecordingMatch();
        }
    }
    
    public void Subscribe(MatchReplayAgent matchReplayAgent)
    {
        activeReplayAgents.Add(matchReplayAgent);
    }

    public void Unsubscribe(MatchReplayAgent matchReplayAgent)
    {
        activeReplayAgents.Remove(matchReplayAgent);
    }

    public void FinishRecordingMatch()
    {
        SaveToJson();
        EditorApplication.isPlaying = false;
    }

    private void SaveToJson()
    {
        agentsSerializableData = new AgentData.AgentSerializableData[activeReplayAgents.Count + inactiveReplayAgents.Count];
        for (var index = 0; index < activeReplayAgents.Count; index++)
        {
            agentsSerializableData[index] = activeReplayAgents[index].AgentData.Serialize();
        }

        var openSceneName = SceneManager.GetActiveScene().name;
        
        var matchData = new MatchData.MatchSerializableData
        {
            SceneName = openSceneName,
            AgentsSerializableData = agentsSerializableData,
            MatchDuration = MatchElapsedTime,
            DecimalPrecision = DecimalPrecision
        };

        var json = JsonUtility.ToJson(matchData);
        var timeSpan = new TimeSpan(0, 0, (int)MatchElapsedTime);
        Debug.Log($"Json: {json}");
        var date = DateTime.Now.Year +
                   "-" + DateTime.Now.Month +
                   "-" + DateTime.Now.Day +
                   "__" + DateTime.Now.Hour +
                   "-" + DateTime.Now.Minute +
                   "-" + DateTime.Now.Second +
                   "__" + (int)timeSpan.TotalMinutes +
                   "-" + (int)timeSpan.TotalSeconds;
        System.IO.File.WriteAllText("Assets/MatchReplay/MatchReplay-" + date + ".json", json);
    }
}
