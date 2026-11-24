using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoadPathCameraAnimation : MonoBehaviour
{

    public List<Vector3> pathPoints;
    public GameObject group;
    int index = 0;
    float counter = 0f;
    public float camSpeed = 14f;
    public float camRotationSpeed = 4f;
    public float inputCooldown = 5f;
    Vector3 targetPosition;
    Vector3 camPos;
    Vector3 offset = Vector3.up * 2f;

    public int safeNumOfSegments = 10;

    public Animator DipToBlackCanvas;
    public CanvasGroup gameplayUiCanvasGRP;

    public Text introCinematicLevelNameText;
    public Text introCinematicLevelDescriptionText;
    public Text introCinematicLevelNumberText;

    bool endSeq = false;

    public void UpdateIntroCinematicText(string levelName, string description, string levelNumber)
    {
        introCinematicLevelNameText.text = levelName;
        introCinematicLevelDescriptionText.text = description;
        introCinematicLevelNumberText.text = levelNumber;
    }

    void Start()
    {
        DipToBlackCanvas.SetBool("black", true);
        if(GameRoot.Instance.UICanvas.shouldSkipTapeTransition || SceneManager.GetActiveScene().name == "Tutorial")
        {
            SkipIntro();
        }
        else
        {
            DisableGameplay();
        }
    }

    void Update()
    {
        if(endSeq)
        {
            group.transform.Translate(transform.forward * Time.deltaTime * camSpeed);
            return;
        }
        if (inputCooldown < 0f)
        {
            if(RewindInput.Instance.GetButtonDown(RewindInputActions.CONFIRM))
            {
                EnableGameplay();
            }
            DipToBlackCanvas.SetTrigger("startbutton");
        }
        else
        {
            inputCooldown -= Time.deltaTime;
        }

        if (counter > 1f)
        {
            if (targetPosition == Vector3.zero)
            {
                if(DipToBlackCanvas.GetBool("black"))
                    DipToBlackCanvas.SetBool("black", false);
                targetPosition = pathPoints[index] + offset;
                camPos = targetPosition;
                group.transform.position = camPos;
            }

            camPos = Vector3.MoveTowards(camPos, targetPosition, Time.deltaTime*(camSpeed+2f));

            group.transform.Translate(transform.forward * Time.deltaTime* camSpeed);
            group.transform.forward = Vector3.Lerp(group.transform.forward, (camPos - group.transform.position).normalized,Time.deltaTime * camRotationSpeed);
            //Debug.DrawLine(group.transform.position, targetPosition,Color.red);
            //Debug.DrawLine(camPos, targetPosition, Color.green);

            if (Vector3.Distance(targetPosition, group.transform.position) < 20f)
            {
                if(DipToBlackCanvas.GetBool("black"))
                {
                    DipToBlackCanvas.SetBool("black", false);
                }

                index++;
                if (index > pathPoints.Count - 1 || index > safeNumOfSegments)
                {
                    index = 0;

                    targetPosition = pathPoints[index] + offset;
                    camPos = targetPosition;
                    group.transform.position = camPos;
                    group.transform.forward = Vector3.forward;
                }
                if (index > pathPoints.Count - 2 || index > safeNumOfSegments - 1)
                {
                    DipToBlackCanvas.SetBool("black", true);
                }
                targetPosition = pathPoints[index] + offset;
                camPos = group.transform.position + (group.transform.forward*30f);
            }


        }
        else
        {
            counter += Time.deltaTime;
        }
    }

    public void AddPathPoint(Vector3 point)
    {
        //Vector3 x = point + offset;
        pathPoints.Add(point);
    }

    public void DisableGameplay()
    {
        if (gameplayUiCanvasGRP != null)
            gameplayUiCanvasGRP.alpha = 0f;
        GameManager.Instance.newCamRig.gameObject.SetActive(false);
        GameManager.Instance.longBoardCTRL.DisableGameplay();
        //GameManager.Instance.pauseMenu.canPause = false;
    }
    public void EnableGameplay()
    {
        endSeq = true;
        DipToBlackCanvas.SetTrigger("start");
        StartCoroutine(EnableGameplayDelay());
        
    }

    public void SkipIntro()
    {
        Debug.LogWarning("[RoadPathCameraAnimation.cs] Intro Skipped");
        this.gameObject.SetActive(false);
        if (gameplayUiCanvasGRP != null)
            gameplayUiCanvasGRP.alpha = 1f;
        GameManager.Instance.newCamRig.gameObject.SetActive(true);
        GameManager.Instance.longBoardCTRL.EnableGameplay();
        //GameManager.Instance.pauseMenu.canPause = true;
    }

    IEnumerator EnableGameplayDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool("Closed", true);
        yield return new WaitForSecondsRealtime(1f);
        if (gameplayUiCanvasGRP != null)
            gameplayUiCanvasGRP.alpha = 1f;

        GameManager.Instance.newCamRig.gameObject.SetActive(true);

        // Clean audio listeners after cutscene
        AudioListener[] audioListeners = transform.GetComponentsInChildren<AudioListener>(true);
        for (int i = 0; i < audioListeners.Length; i++)
        {
            Destroy(audioListeners[i]);
            yield return null;
        }

        GameManager.Instance.longBoardCTRL.EnableGameplay();
        //GameManager.Instance.pauseMenu.canPause = true;
        yield return new WaitForSecondsRealtime(0.5f);
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool("Closed", false);
        this.gameObject.SetActive(false);
    }
}
