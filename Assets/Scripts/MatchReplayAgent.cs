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

        public void Tick(float tick)
        {
                if (RecordPosition)
                {
                        positionRecordingTimer -= Time.deltaTime;
                        if (positionRecordingTimer <= 0f)
                        {
                                positionRecordingTimer = PositionRecodingPeriod;
                                AgentData.PositionEvents.Add(new PositionEvent(cachedTransform.position, tick));
                        }  
                }

                if (RecordRotation)
                {
                        rotationRecordingTimer -= Time.deltaTime;
                        if (rotationRecordingTimer <= 0f)
                        {
                                rotationRecordingTimer = RotationRecodingPeriod;
                                AgentData.RotationEvents.Add(new RotationEvent(cachedTransform.rotation, tick));
                        }    
                }
        }
}