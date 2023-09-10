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
        }
        
        public MatchSerializableData Serialize()
        {
                var matchSerializedData = new MatchSerializableData
                {
                        SceneName = SceneName,
                        AgentsSerializableData = new AgentData.AgentSerializableData[AgentsData.Count],
                        MatchDuration = MatchDuration
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

        public MatchData(MatchSerializableData matchSerializableData)
        {
                SceneName = matchSerializableData.SceneName;
                AgentsData = new List<AgentData>();
                MatchDuration = matchSerializableData.MatchDuration;

                foreach (var agentSerializableData in matchSerializableData.AgentsSerializableData)
                {
                        AgentsData.Add(new AgentData
                        {
                                AgentName = agentSerializableData.AgentName,
                                PositionEvents = agentSerializableData.PositionEvents.ToList(),
                                RotationEvents = agentSerializableData.RotationEvents.ToList()
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
        }

        public AgentSerializableData Serialize()
        {
                return new AgentSerializableData
                {
                        AgentName = AgentName,
                        PositionEvents = PositionEvents.ToArray(),
                        RotationEvents = RotationEvents.ToArray()
                };
        }
        
        public string AgentName;
        public List<PositionEvent> PositionEvents;
        public List<RotationEvent> RotationEvents;

        public void Init(string name)
        {
                AgentName = name;
                PositionEvents = new List<PositionEvent>(512);
                RotationEvents = new List<RotationEvent>(512);
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

        protected BaseEvent(float timeStamp, MatchEventType matchEventType)
        {
                TimeStamp = Mathf.RoundToInt(timeStamp * MatchReplayPlayer.DecimalPrecision);
                MatchEventType = matchEventType;
        }
}

[Serializable]
public class PositionEvent : BaseEvent
{
        public int x;
        public int y;
        public int z;

        public PositionEvent(Vector3 position, float timeStamp) : base(timeStamp, MatchEventType.Position)
        {
                x = Mathf.RoundToInt(position.x * MatchReplayPlayer.DecimalPrecision);
                y = Mathf.RoundToInt(position.y * MatchReplayPlayer.DecimalPrecision);
                z = Mathf.RoundToInt(position.z * MatchReplayPlayer.DecimalPrecision);
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
                x = Mathf.RoundToInt(eulerAngles.x * MatchReplayPlayer.DecimalPrecision);
                y = Mathf.RoundToInt(eulerAngles.y * MatchReplayPlayer.DecimalPrecision);
                z = Mathf.RoundToInt(eulerAngles.z * MatchReplayPlayer.DecimalPrecision);
        }
}