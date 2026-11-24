using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckpointClearAnimation : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioSource clinkAudioSource;
    public AudioSource goalSFXAudioSource;
    public AudioClip clinkAudioClip;
    public AudioClip endHighScoreAudioClip;
    public AudioClip endTimeScoreAudioClip;
    public Text scoreText;
    public Text timeText;
    public Text checkpointNumberText;
    public Animator animator;

    public void UpdateLevelNumber(string newNumber)
    {
        checkpointNumberText.text = newNumber;
    }

    public void PlayMusic()
    {
        if (audioSource == null) return;
        
        AudioService.Instance.DipMusicVolume(0.03f, true);
        audioSource.Play();
        
        if (goalSFXAudioSource != null) goalSFXAudioSource.Play();
    }

    public void StopMusic()
    {
        if (audioSource == null) return;
        
        AudioService.Instance.DipMusicVolume(1f, false);
        StartCoroutine(C_DipMusicAndStop(0f, 0.5f));
    }

    public void StartScoreEffect()
    {
        StartCoroutine(C_ScoreEffect());
    }

    IEnumerator C_ScoreEffect()
    {
        int frameCount = 60;
        scoreText.text = "0";
        timeText.text = GameRoot.FloatToTimeFormat(0f);

        float lastSegmentScore = GameManager.Instance.GetLastSegmentScore();
        float lastSegmentTime = GameManager.Instance.GetLastSegmentTime();

        int count = 0;

        for (int i = 0; i < frameCount; i++)
        {
            scoreText.text = string.Format("{0:n0}", Mathf.Lerp(0f, lastSegmentScore, (float)i/(float)frameCount));
            clinkAudioSource.pitch = Mathf.Lerp(0.95f, 1.05f, (float)i / (float)frameCount);
            if (count % 3 == 0) clinkAudioSource.PlayOneShot(clinkAudioClip);
            yield return new WaitForSecondsRealtime(0.0167f);
            count++;
        }
        scoreText.text = string.Format("{0:n0}", lastSegmentScore);
        clinkAudioSource.PlayOneShot(endHighScoreAudioClip);

        yield return new WaitForSecondsRealtime(0.6f);
        count = 0;

        for (int i = 0; i < frameCount; i++)
        {
            timeText.text = GameRoot.FloatToTimeFormat(Mathf.Lerp(0f, lastSegmentTime, (float)i / (float)frameCount));
            clinkAudioSource.pitch = Mathf.Lerp(0.95f, 1.05f, (float)i / (float)frameCount);
            if (count % 3 == 0) clinkAudioSource.PlayOneShot(clinkAudioClip);
            yield return new WaitForSecondsRealtime(0.0167f);
            count++;
        }

        timeText.text = GameRoot.FloatToTimeFormat(lastSegmentTime);
        clinkAudioSource.PlayOneShot(endTimeScoreAudioClip);
    }

    private IEnumerator C_DipMusicAndStop (float targetVolume, float startFadeDuration = 0.4f)
    {
        for (float f = 0f; f < 1f; f += (Time.unscaledDeltaTime / startFadeDuration))
        {
            audioSource.volume = Mathf.Lerp (1f, targetVolume, f);
            yield return null;
        }
        audioSource.volume = targetVolume;
        audioSource.Stop();
        audioSource.volume = 1f;
    }
}
