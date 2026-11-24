using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FullscreenModeButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public List<FullscreenMode> fullscreenModes;

    public Text displayText;
    public Text helpText;
    public AudioSource audioSource;

    public Button applyButton;

    private int currentIndexInAvailableFullscreenModes;
    private int currentIndexInMenu;

    private bool isSelected = false;
    private bool inCooldown = false;
    private bool hasChanged = true;

    private FullscreenMode selectedFullscreenMode;

    public void Initialize()
    {
        #if !UNITY_SWITCH
        FullscreenMode currentFullscreenMode = fullscreenModes.Find(x => x.fullScreenMode == Screen.fullScreenMode);

        currentIndexInAvailableFullscreenModes = fullscreenModes.IndexOf(currentFullscreenMode);
        currentIndexInMenu = currentIndexInAvailableFullscreenModes;

        UpdateFullscreenText(currentFullscreenMode, false);
        hasChanged = false;
        #endif
    }

    private void UpdateFullscreenText(FullscreenMode newFSMode, bool updateUI = true)
    {
        #if !UNITY_SWITCH
        if (!hasChanged && updateUI)
        {
            hasChanged = true;
            applyButton.interactable = true;
        }

        audioSource.Play();
        displayText.text = newFSMode.shortName;
        if (updateUI) helpText.text = newFSMode.helpTextDescription;
        selectedFullscreenMode = newFSMode;
        
        StartCoroutine(C_Cooldown());
        #endif
    }

    private void ShowNextFullscreenMode()
    {
        currentIndexInMenu++;
        if (currentIndexInMenu >= fullscreenModes.Count) currentIndexInMenu = 0;
        UpdateFullscreenText(fullscreenModes[currentIndexInMenu]);
    }

    private void ShowPreviousFullscreenMode()
    {
        currentIndexInMenu--;
        if (currentIndexInMenu < 0) currentIndexInMenu = fullscreenModes.Count - 1;
        UpdateFullscreenText(fullscreenModes[currentIndexInMenu]);
    }

    private void Update()
    {
        if (!isSelected) return;

        if (RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL) <= -0.9f && !inCooldown)
        {
            ShowPreviousFullscreenMode();
        } else if (RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL) >= 0.9f && !inCooldown)
        {
            ShowNextFullscreenMode();
        }
    }

    IEnumerator C_Cooldown()
    {
        inCooldown = true;
        yield return new WaitForSecondsRealtime(0.4f);
        inCooldown = false;
    }

    public FullscreenMode GetSelectedFullscreenMode()
    {
        return selectedFullscreenMode;
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

[System.Serializable]
public class FullscreenMode
{
    public FullScreenMode fullScreenMode;
    public string shortName;
    public string helpTextDescription;
}
