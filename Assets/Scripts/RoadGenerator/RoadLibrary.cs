using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "NewNewRoadLibrary", menuName = "TanukiSunset/NewRoadLibrary", order = 1)]
public class RoadLibrary : ScriptableObject
{
    public DifficultyCategory normalRoads;
    public DriftRoadTurnCategory driftRoads;
    public MechanicsRoadTurnCategory mechanics;
    public KitRoadCategory kit;
    public ItemRoadCategory items;
    public RoadCategory checkpoints;

    public AnimationCurve curvinessCurve;
    public AnimationCurve difficultyCurve;

    [Reorderable] public List<RoadSegmentEvent> roadSegmentEvents;

    public int piecesPerLevel = 80;

    public string roadLibraryClearId;

    private bool alternateCurveBoolean = false;

    public GameObject GetRandomNormalRoad(RoadDifficulty difficulty)
    {
        List<GameObject> resultList = new List<GameObject>();

        switch (difficulty)
        {
            case (RoadDifficulty.EASY):
                {
                    resultList = resultList.Concat(normalRoads.easy.roads).ToList();
                    resultList = resultList.Concat(mechanics.jumps.easy.roads).ToList();
                    resultList = resultList.Concat(mechanics.gates.easy.roads).ToList();
                    resultList = resultList.Concat(mechanics.narrow.easy.roads).ToList();
                }
                break;
            case (RoadDifficulty.MEDIUM):
                {
                    resultList = resultList.Concat(normalRoads.medium.roads).ToList();
                    resultList = resultList.Concat(mechanics.jumps.medium.roads).ToList();
                    resultList = resultList.Concat(mechanics.gates.medium.roads).ToList();
                    resultList = resultList.Concat(mechanics.narrow.medium.roads).ToList();
                }
                break;
            case (RoadDifficulty.HARD):
                {
                    resultList = resultList.Concat(normalRoads.hard.roads).ToList();
                    resultList = resultList.Concat(mechanics.jumps.hard.roads).ToList();
                    resultList = resultList.Concat(mechanics.gates.hard.roads).ToList();
                    resultList = resultList.Concat(mechanics.narrow.hard.roads).ToList();
                }
                break;
        }

        if (resultList.Count <= 0) return null;

        return resultList[Random.Range(0, resultList.Count)];
    }

    public GameObject GetRandomDriftRoad(RoadDifficulty difficulty)
    {
        List<GameObject> resultList = new List<GameObject>();
        alternateCurveBoolean = !alternateCurveBoolean;

        switch (difficulty)
        {
            case (RoadDifficulty.EASY):
                {
                    if (alternateCurveBoolean) resultList = resultList.Concat(driftRoads.left.easy.roads).ToList();
                    else resultList = resultList.Concat(driftRoads.right.easy.roads).ToList();
                }
                break;
            case (RoadDifficulty.MEDIUM):
                {
                    if (alternateCurveBoolean) resultList = resultList.Concat(driftRoads.left.medium.roads).ToList();
                    else resultList = resultList.Concat(driftRoads.right.medium.roads).ToList();
                }
                break;
            case (RoadDifficulty.HARD):
                {
                    if (alternateCurveBoolean) resultList = resultList.Concat(driftRoads.left.hard.roads).ToList();
                    else resultList = resultList.Concat(driftRoads.right.hard.roads).ToList();
                }
                break;
        }

        return resultList[Random.Range(0, resultList.Count)];
    }

    public GameObject GetRandomTape()
    {
        return items.tape.roads[Random.Range(0, items.tape.roads.Count)];
    }

    public GameObject GetRandomObjective()
    {
        return items.objective.roads[Random.Range(0, items.objective.roads.Count)];
    }

    public GameObject GetRandomBitBonus()
    {
        return items.bitbonus.roads[Random.Range(0, items.bitbonus.roads.Count)];
    }

    public GameObject GetRandomCheckpointPiece()
    {
        return checkpoints.roads[Random.Range(0, checkpoints.roads.Count)];
    }

    public int GetTotalPieceNumber()
    {
        int result = piecesPerLevel;
        foreach (RoadSegmentEvent roadSegmentEvent in roadSegmentEvents)
        {
            result += roadSegmentEvent.roadPrefab.Count;
        }
        return result;
    }
}

[System.Serializable]
public class RoadCategory
{
    public RoadDifficulty roadDifficulty;
    public bool enabled;
    public List<GameObject> roads;
}

[System.Serializable]
public class DifficultyCategory
{
    public RoadCategory easy;
    public RoadCategory medium;
    public RoadCategory hard;
}

[System.Serializable]
public class DriftRoadTurnCategory
{
    public DifficultyCategory left;
    public DifficultyCategory right;
}

[System.Serializable]
public class MechanicsRoadTurnCategory
{
    public DifficultyCategory jumps;
    public DifficultyCategory narrow;
    public DifficultyCategory gates;
}

[System.Serializable]
public class KitRoadCategory
{
    public RoadCategory pickups;
    public RoadCategory dropoffs;
}

[System.Serializable]
public class ItemRoadCategory
{
    public RoadCategory tape;
    public RoadCategory objective;
    public RoadCategory bitbonus;
}

public enum RoadDifficulty
{
    EASY,
    MEDIUM,
    HARD
}
