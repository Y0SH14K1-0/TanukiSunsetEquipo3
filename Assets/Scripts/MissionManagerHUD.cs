using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManagerHUD : MonoBehaviour
{
    public List<Mission> possibleMissions;
    public ObjectiveNotification objectiveNotification;

    private Mission currentMission;
    private bool currentlyOnAMission = false;

    public void StartMission()
    {
        if (currentlyOnAMission)
        {
            StartCoroutine(C_SwitchMissions());
            return;
        }
        currentMission = possibleMissions[Random.Range(0, possibleMissions.Count)];
        currentMission.currentValue = 0;
        currentlyOnAMission = true;
        objectiveNotification.EnableNotification(currentMission);
    }

    public void UpdateMissionProgress(MissionType type, int count)
    {
        if (!currentlyOnAMission || currentMission == null || currentMission.type != type) return;

        currentMission.IncreaseMissionCurrentValue(count);
        objectiveNotification.UpdateNotificationProgress(currentMission);
        if (currentMission.isComplete) EndMission();
    }

    public Mission GetCurrentMission()
    {
        return currentMission;
    }

    private void EndMission()
    {
        GameManager.Instance.scoreSystem.InstantTrick(currentMission.pointsRewardAmount,LocalizationManager.GetLocalizedUIString("ui-key-objective-complete"));// "OBJECTIVE COMPLETE");
        currentlyOnAMission = false;
        currentMission = null;
    }

    IEnumerator C_SwitchMissions()
    {
        objectiveNotification.DismissNotification();
        currentlyOnAMission = false;
        currentMission = null;
        yield return new WaitForSecondsRealtime(1f);
        StartMission();
    }
}

[System.Serializable]
public class Mission
{
    public MissionType type;
    public int targetValue;
    public string descriptionFormat;
    public int pointsRewardAmount;
    [HideInInspector] public int currentValue;
    [HideInInspector] public bool isComplete = false;

    public Mission(MissionType type, int targetValue)
    {
        this.type = type;
        this.targetValue = targetValue;
    }

    public void IncreaseMissionCurrentValue(int value)
    {
        currentValue += value;
        if (currentValue >= targetValue) isComplete = true;
    }
}

public enum MissionType
{
    NearMissCount,
    TightSqueezeCount,
    EdgeLordCount,
    SingleDrift,
    AirTimeCount,
    DriftTimeCount,
    TapeCount,
    RescueKitCount,
    BitTwoFer,
    OneEighty,
    ThreeFiddy,
}
