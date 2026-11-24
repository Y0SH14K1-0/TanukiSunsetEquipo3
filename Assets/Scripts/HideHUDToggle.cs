using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Toggle))]

public class HideHUDToggle : MonoBehaviour
{
    public Text vsyncButtonText;
    public AudioSource toggleAudioSource;
    public AudioClip onAudioClip;
    public AudioClip offAudioClip;

    private Toggle _toggle;

    public void Initialize()
    {
        _toggle = GetComponent<Toggle>();
        SetToggleMute(PlayerPrefs.GetString("HideHUD", "false") == "true");
    }

    public void SetToggleMute(bool value)
    {
        vsyncButtonText.text = value ? LocalizationManager.GetLocalizedUIString("ui-key-on") : LocalizationManager.GetLocalizedUIString("ui-key-off");
        PlayerPrefs.SetString("HideHUD", value ? "true" : "false");
        _toggle.isOn = value;
    }

    public void SetToggle(bool value)
    {
        SetToggleMute(value);
        toggleAudioSource.PlayOneShot(value ? onAudioClip : offAudioClip);
    }
}
