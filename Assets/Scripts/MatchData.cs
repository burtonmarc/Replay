using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class MatchData
{
        [Serializable]
        public class MatchSerializableData 
        {
                public string SceneName;
                public AgentData.AgentSerializableData[] AgentsSerializableData; // List of agents of the match
                public float MatchDuration;
                public int DecimalPrecision;
        }
        
        public MatchSerializableData Serialize()
        {
                var matchSerializedData = new MatchSerializableData
                {
                        SceneName = SceneName,
                        AgentsSerializableData = new AgentData.AgentSerializableData[AgentsData.Count],
                        MatchDuration = MatchDuration,
                        DecimalPrecision = MatchReplayRecorder.DecimalPrecision
                };
                for (var index = 0; index < AgentsData.Count; index++)
                {
                        matchSerializedData.AgentsSerializableData[index] = AgentsData[index].Serialize();
                }

                return matchSerializedData;
        }
        
        public string SceneName;
        public List<AgentData> AgentsData;
        public float MatchDuration;
        public int DecimalPrecision;

        public MatchData(MatchSerializableData matchSerializableData)
        {
                SceneName = matchSerializableData.SceneName;
                AgentsData = new List<AgentData>();
                MatchDuration = matchSerializableData.MatchDuration;
                DecimalPrecision = matchSerializableData.DecimalPrecision;

                foreach (var agentSerializableData in matchSerializableData.AgentsSerializableData)
                {
                        AgentsData.Add(new AgentData
                        {
                                AgentName = agentSerializableData.AgentName,
                                PositionEvents = agentSerializableData.PositionEvents.ToList(),
                                RotationEvents = agentSerializableData.RotationEvents.ToList(),
                                EliminationEvents = agentSerializableData.EliminationEvents.ToList()
                        });
                }
        }
}

public class AgentData
{
        [Serializable]
        public class AgentSerializableData
        {
                public string AgentName;
                public PositionEvent[] PositionEvents;
                public RotationEvent[] RotationEvents;
                public EliminationEvent[] EliminationEvents;
        }

        public AgentSerializableData Serialize()
        {
                return new AgentSerializableData
                {
                        AgentName = AgentName,
                        PositionEvents = PositionEvents.ToArray(),
                        RotationEvents = RotationEvents.ToArray(),
                        EliminationEvents = EliminationEvents.ToArray()
                };
        }
        
        public string AgentName;
        public List<PositionEvent> PositionEvents;
        public List<RotationEvent> RotationEvents;
        public List<EliminationEvent> EliminationEvents;
        
        // Editor Variables
        public bool Mute;

        public void Init(string name)
        {
                AgentName = name;
                PositionEvents = new List<PositionEvent>(1024);
                RotationEvents = new List<RotationEvent>(1024);
                EliminationEvents = new List<EliminationEvent>(64);
        }
}

public enum MatchEventType
{
        Position,
        Rotation,
        Spawn,
        Elimination
}

public abstract class BaseEvent
{
        public float TimeStamp;
        public MatchEventType MatchEventType;

        protected BaseEvent(float currentTime, MatchEventType matchEventType)
        {
                TimeStamp = Mathf.RoundToInt(currentTime * MatchReplayRecorder.DecimalPrecision);
                MatchEventType = matchEventType;
        }
}

[Serializable]
public class PositionEvent : BaseEvent
{
        public int x;
        public int y;
        public int z;

        public PositionEvent(Vector3 position, float currentTime) : base(currentTime, MatchEventType.Position)
        {
                x = Mathf.RoundToInt(position.x * MatchReplayRecorder.DecimalPrecision);
                y = Mathf.RoundToInt(position.y * MatchReplayRecorder.DecimalPrecision);
                z = Mathf.RoundToInt(position.z * MatchReplayRecorder.DecimalPrecision);
        }

        public Vector3 Pos => new(x, y, z);
}

[Serializable]
public class RotationEvent : BaseEvent
{
        public float x;
        public float y;
        public float z;
        
        public RotationEvent(Quaternion rotation, float timeStamp) : base(timeStamp, MatchEventType.Rotation)
        {
                var eulerAngles = rotation.eulerAngles;
                x = Mathf.RoundToInt(eulerAngles.x * MatchReplayRecorder.DecimalPrecision);
                y = Mathf.RoundToInt(eulerAngles.y * MatchReplayRecorder.DecimalPrecision);
                z = Mathf.RoundToInt(eulerAngles.z * MatchReplayRecorder.DecimalPrecision);
        }
}

[Serializable]
public class EliminationEvent : BaseEvent
{
        public float x;
        public float y;
        public float z;

        public EliminationEvent(Vector3 position, float currentTime) : base(currentTime, MatchEventType.Elimination)
        {
                x = Mathf.RoundToInt(position.x * MatchReplayRecorder.DecimalPrecision);
                y = Mathf.RoundToInt(position.y * MatchReplayRecorder.DecimalPrecision);
                z = Mathf.RoundToInt(position.z * MatchReplayRecorder.DecimalPrecision);
        }
        
        public Vector3 Pos => new(x, y, z);
}