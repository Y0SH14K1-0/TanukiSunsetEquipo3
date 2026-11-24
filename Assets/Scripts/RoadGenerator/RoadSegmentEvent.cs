using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadSegmentEvent
{
    public int spawnPieceNumber;
    public List<GameObject> roadPrefab;
    public RoadSegmentEventType eventType = RoadSegmentEventType.NORMAL;
}

[System.Serializable]
public enum RoadSegmentEventType
{
    NORMAL,
    SOFT_CHECKPOINT,
    CHECKPOINT,
}