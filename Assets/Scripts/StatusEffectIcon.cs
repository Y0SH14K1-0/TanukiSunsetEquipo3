using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIcon : MonoBehaviour
{
    public Image iconImage;
    public Image circleFillImage;
    public Animator animator;

    private bool isEnabled = false;

    public void UpdateCircleFillAmount(float fillAmount)
    {
        circleFillImage.fillAmount = fillAmount;
    }

    public void SetEnabled(bool value)
    {
        isEnabled = value;
        animator.SetBool("Enabled", value);
    }

    public bool IsEnabled()
    {
        return isEnabled;
    }
}
