using System;
using UnityEngine;

[Serializable]
public class MatchReplayAgent : MonoBehaviour
{
        public bool RecordPosition;
        public bool RecordRotation;
        public float PositionRecodingPeriod;
        public float RotationRecodingPeriod;
        public string AgentName;
        
        private float positionRecordingTimer;
        private float rotationRecordingTimer;
        private Transform cachedTransform;
        
        public AgentData AgentData;

        private void Start()
        {
                cachedTransform = transform;
                AgentData = new AgentData();
                AgentData.Init(AgentName);
                
                MatchReplayRecorder.Instance.Subscribe(this);
        }

        private void OnDestroy()
        {
                MatchReplayRecorder.Instance.Unsubscribe(this);
        }

        public void CustomUpdate()
        {
                if (RecordPosition)
                {
                        positionRecordingTimer -= Time.deltaTime;
                        if (positionRecordingTimer <= 0f)
                        {
                                positionRecordingTimer = PositionRecodingPeriod;
                                AgentData.PositionEvents.Add(new PositionEvent(cachedTransform.position, MatchReplayRecorder.MatchElapsedTime));
                        }  
                }

                if (RecordRotation)
                {
                        rotationRecordingTimer -= Time.deltaTime;
                        if (rotationRecordingTimer <= 0f)
                        {
                                rotationRecordingTimer = RotationRecodingPeriod;
                                AgentData.RotationEvents.Add(new RotationEvent(cachedTransform.rotation, MatchReplayRecorder.MatchElapsedTime));
                        }    
                }
        }

        public void OnElimination(Vector3 position)
        {
                Debug.Log($"Elimination at position: {position}");
                AgentData.EliminationEvents.Add(new EliminationEvent(position, MatchReplayRecorder.MatchElapsedTime));
        }
}