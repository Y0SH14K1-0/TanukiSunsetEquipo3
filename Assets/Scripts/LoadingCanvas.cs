using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingCanvas : MonoBehaviour
{
    public GameObject canvasObject;
    public Animator SavingNowAnimator;
    public Animator SaveNoticeAnimator;
    public Animator loadingCanvasAnimator;

    private bool _hasSeenSaveNotice = false;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(C_LoadScene(sceneName));
    }

    IEnumerator C_LoadScene(string sceneName)
    {
        canvasObject.SetActive(true);
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return StartCoroutine(C_CheckIfShowSaveNotice());
        yield return new WaitForSecondsRealtime(0.333f);
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        loadingCanvasAnimator.SetTrigger("end");
        yield return new WaitForSecondsRealtime(0.333f);
        canvasObject.SetActive(false);
    }

    public void LoadScene(int sceneId)
    {
        StartCoroutine(C_LoadScene(sceneId));
    }

    IEnumerator C_LoadScene(int sceneId)
    {
        canvasObject.SetActive(true);
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        yield return StartCoroutine(C_CheckIfShowSaveNotice());
        yield return new WaitForSecondsRealtime(0.333f);
        yield return SceneManager.LoadSceneAsync(sceneId);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        loadingCanvasAnimator.SetTrigger("end");
        yield return new WaitForSecondsRealtime(0.333f);
        canvasObject.SetActive(false);
    }

    IEnumerator C_CheckIfShowSaveNotice()
    {
        if (!_hasSeenSaveNotice)
        {
            SaveNoticeAnimator.SetBool("SaveNotice", true);
            _hasSeenSaveNotice = true;
            yield return new WaitForSecondsRealtime(6f);
        }
        SaveNoticeAnimator.SetBool("SaveNotice", false);
        //yield break;
    }
}
