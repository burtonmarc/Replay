using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MatchReplayPlayer : MonoBehaviour
{
    private string path;

    public List<MatchData.MatchSerializableData> allMatchesData = new();

    [HideInInspector] public float replayStartPercentage;
    [HideInInspector] public float replayEndPercentage;

    public void LoadMatchDataFiles()
    {
        Debug.Log("Load Match Data Files");
        var fileNames = Directory.GetFiles("Assets/MatchReplay", "*.json");

        allMatchesData.Clear();
        foreach (var fileName in fileNames)
        {
            var json = File.ReadAllText(fileName);
            var matchData = JsonUtility.FromJson<MatchData.MatchSerializableData>(json);
            allMatchesData.Add(matchData);
        }
    }
}