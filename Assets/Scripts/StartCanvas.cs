using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCanvas : MonoBehaviour
{
    [SerializeField] private AudioSource rewindLogoAudioSource;
    
    void Start()
    {
        //StartCoroutine(C_Start());
    }

    IEnumerator C_Start()
    {
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool("Closed", true);
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetTrigger("ForceClosed");
        yield return new WaitForSecondsRealtime(1f);
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool("Closed", false);
        
    }

    public void LoadNextScene()
    {
        GameRoot.Instance.loadingCanvas.LoadScene("SplashScreen");
    }
    
    public void PlayAudioLogo()
    {
        if (rewindLogoAudioSource != null) rewindLogoAudioSource.Play();
    }
}
