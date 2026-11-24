using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    bool ChangeScene = false;
    public bool canWork = false;
    public Animator anim;
    public AudioSource buttonSound;
    public ParticleSystem buttonEffect;

    private void Start()
    {
        AudioService.Instance.MuteMusic(false);
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool("Closed", false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!canWork || ChangeScene) return;

        if((RewindInput.Instance.GetButton(RewindInputActions.LEFT_BUMPER) && RewindInput.Instance.GetButton(RewindInputActions.RIGHT_BUMPER)) || RewindInput.Instance.GetButton(RewindInputActions.CONFIRM))
        {
            ChangeScene = true;
            anim.SetTrigger("outro");
            if (buttonSound != null) buttonSound.Play();
            if (buttonEffect != null) buttonEffect.Play();
            StartCoroutine(LoadScene());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit(0);
        }
    }
    
    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(2f);
        GameRoot.Instance.loadingCanvas.LoadScene("LoadSaveFile");
        /*
        if (SaveSystem.CurrentSaveData().seenStoreCutscene)
            GameRoot.Instance.loadingCanvas.LoadScene("Store");
        else
        {
            LevelManager.Instance.SetCheckpoint(0);
            GameRoot.Instance.loadingCanvas.LoadScene("Tutorial");
        }*/
        GameRoot.Instance.UICanvas.blackScreenCanvasGroup.FadeTo(this, 1f, 0.5f);
    }
}
