using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Toggle))]

public class RumbleToggle : MonoBehaviour
{
    public Text vsyncButtonText;
    public AudioSource toggleAudioSource;
    public AudioClip onAudioClip;
    public AudioClip offAudioClip;

    private Toggle _toggle;

    public void Initialize()
    {
        _toggle = GetComponent<Toggle>();
        SetToggleMute(PlayerPrefs.GetString("EnableRumble", "false") == "true");
    }

    public void SetToggleMute(bool value)
    {
        vsyncButtonText.text = value ? LocalizationManager.GetLocalizedUIString("ui-key-on") : LocalizationManager.GetLocalizedUIString("ui-key-off");
        PlayerPrefs.SetString("EnableRumble", value ? "true" : "false");
        _toggle.isOn = value;
    }

    public void SetToggle(bool value)
    {
        SetToggleMute(value);
        toggleAudioSource.PlayOneShot(value ? onAudioClip : offAudioClip);
    }
}
