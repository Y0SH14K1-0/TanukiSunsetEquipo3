using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    public CanvasGroup startMenuButtonsCanvasGroup;
    public CanvasGroup howToPlayCanvasGroup;
    public GameObject firstSelectedGameObject;
    public AudioMixer audioMixer;
    public AudioSource sfxTestAudioSource;
    public Animator circleTransitionAnimator;
    public AudioSource musicAudioSource;
    public Animator howToPlayAnimator;
    public AudioSource sfxClickAudioSource;

    void Start()
    {
        StartCoroutine(C_Start());
    }

    IEnumerator C_Start()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        musicAudioSource.Play();
    }

    public void OnIntroFinished()
    {
        startMenuButtonsCanvasGroup.blocksRaycasts = true;
        startMenuButtonsCanvasGroup.interactable = true;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedGameObject);
    }

    public void OnStartPressed()
    {
        StartCoroutine(C_StartGame());
    }

    IEnumerator C_StartGame()
    {
        circleTransitionAnimator.SetBool("Closed", true);
        for (float f = 0f; f <= 1f; f += Time.unscaledDeltaTime/2f)
        {
            musicAudioSource.volume = 1f-f;
            yield return null;
        }
        musicAudioSource.volume = 0f;
        SceneManager.LoadScene(4);
    }

    public void OnExit()
    {
        StartCoroutine(C_OnExit());
    }

    IEnumerator C_OnExit()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            circleTransitionAnimator.SetBool("Closed", true);
            for (float f = 0f; f <= 1f; f += Time.unscaledDeltaTime/2f)
            {
                musicAudioSource.volume = 1f-f;
                yield return null;
            }
            musicAudioSource.volume = 0f;
            Application.Quit();
        }
    }
}
