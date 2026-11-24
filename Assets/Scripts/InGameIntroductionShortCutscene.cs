using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameIntroductionShortCutscene : MonoBehaviour
{
    // Start is called before the first frame update
    public DipTimeScaleFunction DTSF;
    public GameObject objectCamera;
    public GameObject objectCanvas;
    void Start()
    {
        StartCoroutine(ActivateCutscene());
    }

    // Update is called once per frame

    IEnumerator ActivateCutscene()
    {
        yield return new WaitForSeconds(1f);
        objectCamera.SetActive(true);
        objectCanvas.SetActive(true);
        DTSF.DipTimeScale(5f);
        yield return new WaitForSecondsRealtime(4f);
        objectCamera.SetActive(false);
        objectCanvas.SetActive(false);
    }
}
