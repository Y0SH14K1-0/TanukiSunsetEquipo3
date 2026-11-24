using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevelFunction : MonoBehaviour
{    
    public void LoadLevelNumber(int levelIndex)
    {
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool("Closed", true);
        GameRoot.Instance.loadingCanvas.LoadScene(levelIndex);
        GameRoot.Instance.hudCanvas.DisableBar();
    }
}
