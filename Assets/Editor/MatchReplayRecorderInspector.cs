using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(MatchReplayRecorder))]
    public class MatchReplayRecorderInspector : UnityEditor.Editor
    {
        private MatchReplayRecorder matchReplayRecorder;
        
        private void Awake()
        {
            matchReplayRecorder = target as MatchReplayRecorder;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Finish Recording Match"))
            {
                matchReplayRecorder.FinishRecordingMatch();
                EditorApplication.isPlaying = false;
            }
        }
    }
}