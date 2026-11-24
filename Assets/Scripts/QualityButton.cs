using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QualityButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Text resolutionText;
    public AudioSource audioSource;

    private int currentIndexInAvailableQualities;
    private int currentIndexInMenu;

    private bool isSelected = false;
    private bool inCooldown = false;
    
    public void Initialize()
    {
        currentIndexInAvailableQualities = QualitySettings.GetQualityLevel();
        currentIndexInMenu = currentIndexInAvailableQualities;
        UpdateQualityText();
    }

    private void UpdateQualityText(bool updateUI = true)
    {
        audioSource.Play();
        resolutionText.text = string.Format("{0}", QualitySettings.names[currentIndexInAvailableQualities].ToUpper());

        StartCoroutine(C_Cooldown());
    }

    private void Update()
    {
        if (!isSelected) return;

        if (RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL) <= -0.9f && !inCooldown)
        {
            currentIndexInMenu--;
            if (currentIndexInMenu < 0) currentIndexInMenu = QualitySettings.names.Length-1;

            QualitySettings.SetQualityLevel(currentIndexInMenu, true);
            currentIndexInAvailableQualities = currentIndexInMenu;
            UpdateQualityText();

        } else if (RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL) >= 0.9f && !inCooldown)
        {
            currentIndexInMenu++;
            if (currentIndexInMenu >= QualitySettings.names.Length) currentIndexInMenu = 0;

            QualitySettings.SetQualityLevel(currentIndexInMenu, true);
            currentIndexInAvailableQualities = currentIndexInMenu;
            UpdateQualityText();
        }
    }

    IEnumerator C_Cooldown()
    {
        inCooldown = true;
        yield return new WaitForSecondsRealtime(0.4f);
        inCooldown = false;
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
