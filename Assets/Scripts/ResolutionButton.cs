using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResolutionButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Text resolutionText;
    public AudioSource audioSource;

    public Button applyButton;

    private int currentIndexInAvailableResolutions;
    private int currentIndexInMenu;

    private bool isSelected = false;
    private bool inCooldown = false;
    private bool hasChanged = true;

    private Resolution selectedResolution;
    
    public void Initialize()
    {
        Resolution currentRes = Screen.currentResolution;
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (Screen.resolutions[i].Equals(currentRes))
            {
                currentIndexInAvailableResolutions = i;
                break;
            }
        }

        currentIndexInMenu = currentIndexInAvailableResolutions;

        UpdateResolutionText(currentRes, false);
        hasChanged = false;
    }

    private void UpdateResolutionText(Resolution resolution, bool updateUI = true)
    {
        if (!hasChanged && updateUI)
        {
            hasChanged = true;
            applyButton.interactable = true;
        }

        audioSource.Play();
        resolutionText.text = string.Format("{0} X {1} / {2}", resolution.width, resolution.height, resolution.refreshRate);
        selectedResolution = resolution;

        StartCoroutine(C_Cooldown());
    }

    private void ShowNextResolution()
    {
        currentIndexInMenu++;
        if (currentIndexInMenu >= Screen.resolutions.Length) currentIndexInMenu = 0;
        UpdateResolutionText(Screen.resolutions[currentIndexInMenu]);
    }

    private void ShowPreviousResolution()
    {
        currentIndexInMenu--;
        if (currentIndexInMenu < 0) currentIndexInMenu = Screen.resolutions.Length - 1;
        UpdateResolutionText(Screen.resolutions[currentIndexInMenu]);
    }

    private void Update()
    {
        if (!isSelected) return;

        if (RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL) <= -0.9f && !inCooldown)
        {
            ShowPreviousResolution();
        } else if (RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL) >= 0.9f && !inCooldown)
        {
            ShowNextResolution();
        }
    }

    IEnumerator C_Cooldown()
    {
        inCooldown = true;
        yield return new WaitForSecondsRealtime(0.4f);
        inCooldown = false;
    }

    public Resolution GetSelectedResolution()
    {
        return selectedResolution;
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
    }
}
