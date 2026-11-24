using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class RoadGenerator : MonoBehaviour
{
    public RoadSegment startRoadSegment;
    public List<RoadLibrary> levelRoadLibraries;

    private int maxLoadedSegments = 16;
    private Transform currentRoadAnchorTransform;
    private Queue<RoadSegment> roadQueue;

    private int currentPieceNumber = 0;
    private int eventsSpawned = 0;

    private Queue<GameObject> itemQueue;
    private Queue<GameObject> eventQueue;

    private float currentCurveTime;
    private int currentCheckpointID = 0;
    public int startTriggerCountSinceCheckpoint = 0;
    public int startTriggerCountSinceLevelLoaded = 0;

    private bool placedCheckpointPiece = false;

    public RoadPathCameraAnimation roadPathCameraAnimation;
    public Transform roadPiecesParent;

    public string GetCurrentRoadLibraryClearId()
    {
        return levelRoadLibraries[currentCheckpointID].roadLibraryClearId;
    }

    private void Start()
    {
        roadQueue = new Queue<RoadSegment>();
        itemQueue = new Queue<GameObject>();
        eventQueue = new Queue<GameObject>();
        roadQueue.Enqueue(startRoadSegment);
        currentRoadAnchorTransform = startRoadSegment.endPosition.transform;
        currentCheckpointID = 0;

        GenerateInitialRoad();
    }

    // Generate the first pieces of the level, grab the maximum allowed loaded segments by half.
    private void GenerateInitialRoad()
    {
        for (int i = 0; i < Mathf.FloorToInt((float)maxLoadedSegments/2f); i++)
        {
            GenerateNextSegment();
        }
    }

    public void IncreaseStartTriggerCount()
    {
        startTriggerCountSinceCheckpoint++;
        startTriggerCountSinceLevelLoaded++;
    }

    // Triggered by SegmentStartTrigger, puts a new piece at the end and deletes the oldest one
    public void GenerateNextSegment()
    {
        if (eventQueue.Count >= 0) currentPieceNumber++;
        currentCurveTime = (float)currentPieceNumber / (float)levelRoadLibraries[currentCheckpointID].GetTotalPieceNumber();

        GameObject newRoad = null;

        if (currentCurveTime <= 1f)
        {
            FillEventQueue();

            if (eventQueue.Count > 0)
            {
                newRoad = eventQueue.Dequeue();
            }
            else if (itemQueue.Count > 0)
            {
                newRoad = itemQueue.Dequeue();
            }
            else
            {
                if (levelRoadLibraries[currentCheckpointID].piecesPerLevel > 0)
                {

                    float chanceOfCurve = levelRoadLibraries[currentCheckpointID].curvinessCurve.Evaluate(currentCurveTime) *5f;

                    if (Random.Range(0f, 1f) < chanceOfCurve)
                    {
                        newRoad = GetCurvePiece();
                    }
                    else
                    {
                        newRoad = GetRoadPiece();
                    }
                }
            }
        }
        else
        {
            FillEventQueue();
            if (eventQueue.Count <= 0)
            {
                if (!placedCheckpointPiece)
                    newRoad = GetCheckpointPiece();
                else if (!LoadNextLibrary())
                    newRoad = GetRoadPiece();
            } else
            {
                newRoad = eventQueue.Dequeue();
            }
        }

        if (newRoad == null)
        {
            Debug.Log("Null piece of road.");
            return;
        }

        RoadSegment newSegment = Instantiate(newRoad, currentRoadAnchorTransform.position, currentRoadAnchorTransform.rotation, roadPiecesParent).GetComponent<RoadSegment>();
        currentRoadAnchorTransform = newSegment.endPosition.transform;
        roadQueue.Enqueue(newSegment);
        if (roadQueue.Count > maxLoadedSegments)
        {
            RoadSegment segmentToDelete = roadQueue.Dequeue();
            Destroy(segmentToDelete.gameObject);
        }

        if (roadPathCameraAnimation != null)
        {
            roadPathCameraAnimation.AddPathPoint(newSegment.transform.position);
        }
    }

    public bool LoadNextLibrary()
    {
        currentCheckpointID++;
        if (currentCheckpointID >= levelRoadLibraries.Count)
        {
            currentCheckpointID = levelRoadLibraries.Count - 1;
            return false;
        }

        currentPieceNumber = 0;
        eventsSpawned = 0;
        placedCheckpointPiece = false;
        return true;
    }

    private void FillEventQueue()
    {
        List<RoadSegmentEvent> events = levelRoadLibraries[currentCheckpointID].roadSegmentEvents.Where(x => x.spawnPieceNumber <= currentPieceNumber).ToList();

        for (int i = eventsSpawned; i < events.Count; i++)
        {
            foreach (GameObject roadPrefab in events[i].roadPrefab)
            {
                eventQueue.Enqueue(roadPrefab);
            }
            eventsSpawned++;
        }
    }

    private GameObject GetCurvePiece()
    {
        GameObject newRoad = null;
        float difficultyChance = levelRoadLibraries[currentCheckpointID].difficultyCurve.Evaluate(currentCurveTime);

        if (difficultyChance < 1f)
        {
            float randomDifficultyFactor = Random.Range(0, 1f);
            if (randomDifficultyFactor < Mathf.Clamp(difficultyChance, 0f, 1f))
            {
                newRoad = levelRoadLibraries[currentCheckpointID].GetRandomDriftRoad(RoadDifficulty.EASY);
            }
            else
            {
                newRoad = levelRoadLibraries[currentCheckpointID].GetRandomDriftRoad(RoadDifficulty.MEDIUM);
            }
        }
        else
        {
            float randomDifficultyFactor = Random.Range(0, 1f);
            if (randomDifficultyFactor < Mathf.Clamp(difficultyChance - 1f, 0f, 1f))
            {
                newRoad = levelRoadLibraries[currentCheckpointID].GetRandomDriftRoad(RoadDifficulty.MEDIUM);
            }
            else
            {
                newRoad = levelRoadLibraries[currentCheckpointID].GetRandomDriftRoad(RoadDifficulty.HARD);
            }
        }

        return newRoad;
    }

    private GameObject GetRoadPiece()
    {
        GameObject newRoad = null;
        float difficultyChance = levelRoadLibraries[currentCheckpointID].difficultyCurve.Evaluate(currentCurveTime) * 2f;

        if (difficultyChance < 1f)
        {
            float randomDifficultyFactor = Random.Range(0, 1f);
            if (randomDifficultyFactor < Mathf.Clamp(difficultyChance, 0f, 1f))
            {
                newRoad = levelRoadLibraries[currentCheckpointID].GetRandomNormalRoad(RoadDifficulty.EASY);
            }
            else
            {
                newRoad = levelRoadLibraries[currentCheckpointID].GetRandomNormalRoad(RoadDifficulty.MEDIUM);
            }
        }
        else
        {
            float randomDifficultyFactor = Random.Range(0, 1f);
            if (randomDifficultyFactor < Mathf.Clamp(difficultyChance - 1f, 0f, 1f))
            {
                newRoad = levelRoadLibraries[currentCheckpointID].GetRandomNormalRoad(RoadDifficulty.MEDIUM);
            }
            else
            {
                newRoad = levelRoadLibraries[currentCheckpointID].GetRandomNormalRoad(RoadDifficulty.HARD);
            }
        }

        return newRoad;
    }

    private GameObject GetCheckpointPiece()
    {
        placedCheckpointPiece = true;
        return levelRoadLibraries[currentCheckpointID].GetRandomCheckpointPiece();
    }
}