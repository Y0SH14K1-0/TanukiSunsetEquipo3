using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveNotification : MonoBehaviour
{
    public Animator animator;
    public Text objectiveDescriptionText;
    public Text objectiveRewardText;

    public void EnableNotification(Mission mission)
    {
        animator.SetBool("Complete", false);
        animator.SetBool("Enabled", true);
        objectiveDescriptionText.text = string.Format(LocalizationManager.GetLocalizedUIString(mission.descriptionFormat), mission.currentValue, mission.targetValue);
        objectiveRewardText.text = string.Format("{0} "+LocalizationManager.GetLocalizedUIString("ui-key-points"), mission.pointsRewardAmount);
    }

    public void UpdateNotificationProgress(Mission mission)
    {
        objectiveDescriptionText.text = string.Format(LocalizationManager.GetLocalizedUIString(mission.descriptionFormat), mission.currentValue, mission.targetValue);
        if (mission.isComplete)
        {
            animator.SetBool("Complete", true);
            Invoke("DismissNotification", 1f);
        } else
        {
            animator.SetTrigger("Update");
        }
    }

    public void DismissNotification()
    {
        animator.SetBool("Complete", false);
        animator.SetBool("Enabled", false);
    }
}
