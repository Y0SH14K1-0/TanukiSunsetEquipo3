using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniPlayer : MonoBehaviour
{
    public GameObject playIconImageObject;
    public GameObject pauseIconImageObject;
    public Text displayText;

    private bool _isPaused = false;

    private void Start()
    {
        AudioService.Instance.onSongChange += RefreshUI;
    }

    private void OnDestroy()
    {
        AudioService.Instance.onSongChange -= RefreshUI;
    }

    public void RefreshUI(Song newSong)
    {
        playIconImageObject.gameObject.SetActive(_isPaused);
        pauseIconImageObject.gameObject.SetActive(!_isPaused);
        UpdateText(newSong.name);
    }

    public void NextSong()
    {
        AudioService.Instance.PlayNextSongFast();
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        AudioService.Instance.Pause(_isPaused);
        //RefreshUI(AudioService.Instance.GetCurrentSong());
    }

    public void UpdateText(string newText)
    {
        displayText.text = newText;
    }
}
