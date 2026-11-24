using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoModeCameraMovement : MonoBehaviour
{
    public GameObject camRig;
    public GameObject camLookAt;
    public GameObject camHolder;
    float HorRotSpeed = 50f;
    float VerRotSpeed = 50f;
    public Camera cam;
    public Camera BGcam;
    float camDist = -5f;
    float camFOV = 60f;
    Vector2 lookAt;
    float camZrot = 0f;
    public bool PhotoModeOn = false;

    public float canvasFadeTime = 3f;
    float canvasTimer = 0;
    public Canvas C_PhotomodeCanvas;
    public CanvasGroup CG_PhotomodeControls;
    public GameObject LogosGroup;
    List<GameObject> Logos = new List<GameObject>();
    int logoIndex = 0;

    private void Start()
    {
        foreach (Transform child in LogosGroup.gameObject.transform)
        {
            Logos.Add(child.gameObject);
        }
        foreach (GameObject logo in Logos)
        {
            logo.gameObject.SetActive(false);
        }
    }
    void Update()
    {
        if (Time.timeScale == 0f && GameManager.Instance.isPaused)
        {
            if (PhotoModeOn)
            {
                if (RewindInput.Instance.GetButton(RewindInputActions.CONFIRM) || RewindInput.Instance.GetButton(RewindInputActions.JUMP) || RewindInput.Instance.GetButton(RewindInputActions.SWITCH))
                {
                    if (RewindInput.Instance.GetButton(RewindInputActions.JUMP))
                    {
                        camFOV += -RewindInput.Instance.GetAxisRaw(RewindInputActions.VERTICAL) * Time.unscaledDeltaTime * 20f;
                        camFOV = Mathf.Clamp(camFOV, 20f, 90f);
                        cam.fieldOfView = camFOV;
                        BGcam.fieldOfView = camFOV;
                    }
                    else if (RewindInput.Instance.GetButton(RewindInputActions.CONFIRM))
                    {
                        camDist += RewindInput.Instance.GetAxisRaw(RewindInputActions.VERTICAL) * Time.unscaledDeltaTime * 10f;
                        camDist = Mathf.Clamp(camDist, -12f, -2f);
                        camHolder.transform.localPosition = new Vector3(0f, 0f, camDist);

                        camZrot += -RewindInput.Instance.GetAxisRaw(RewindInputActions.HORIZONTAL) * Time.unscaledDeltaTime * 20f;
                        camZrot = Mathf.Clamp(camZrot, -90f, 90f);
                        cam.transform.localEulerAngles = new Vector3(0f, 0f, camZrot);
                    }
                    else if (RewindInput.Instance.GetButton(RewindInputActions.SWITCH))
                    {
                        lookAt += new Vector2(RewindInput.Instance.GetAxisRaw(RewindInputActions.HORIZONTAL) * Time.unscaledDeltaTime * 10f, RewindInput.Instance.GetAxisRaw(RewindInputActions.VERTICAL) * Time.unscaledDeltaTime * 10f);
                        lookAt = new Vector2(Mathf.Clamp(lookAt.x, -5f, 5f), Mathf.Clamp(lookAt.y, -3f, 3f));
                        camLookAt.transform.localPosition = new Vector3(lookAt.x, lookAt.y, 0f);
                    }

                    canvasTimer = canvasFadeTime;
                }
                else
                {
                    camRig.transform.RotateAround(camRig.transform.position, transform.up, -RewindInput.Instance.GetAxisRaw(RewindInputActions.HORIZONTAL) * Time.unscaledDeltaTime * HorRotSpeed);
                    camRig.transform.RotateAround(camRig.transform.position, camRig.transform.right, RewindInput.Instance.GetAxisRaw(RewindInputActions.VERTICAL) * Time.unscaledDeltaTime * VerRotSpeed);
                    if(RewindInput.Instance.GetButtonDown(RewindInputActions.JOYSTICK_PRESS))
                    {
                        ToggleLogo();
                    }
                    if(new Vector2(RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL), RewindInput.Instance.GetAxisRaw(RewindInputActions.VERTICAL)).magnitude > 0.2f)
                        canvasTimer = canvasFadeTime;
                }

                if (canvasTimer > 0f)
                {
                    CG_PhotomodeControls.alpha = Mathf.Clamp01(canvasTimer);
                    canvasTimer -= Time.unscaledDeltaTime;
                }

                camHolder.transform.LookAt(camLookAt.transform.position);

                if (RewindInput.Instance.GetButton(RewindInputActions.BACK))
                {
                    PhotoModeOn = false;
                    //GameManager.Instance.pauseMenu.HideGeneralCanvas(true);
                    cam.gameObject.SetActive(false);
                    C_PhotomodeCanvas.gameObject.SetActive(false);
                }
            }
        }
    }
    public void TurnOnPhotomode()
    {
        if(PhotoModeOn)
           return; 
        PhotoModeOn = true;
        //GameManager.Instance.pauseMenu.HideGeneralCanvas(false);
        cam.gameObject.SetActive(true);
        C_PhotomodeCanvas.gameObject.SetActive(true);
    }

    void ToggleLogo()
    {
        foreach (GameObject logo in Logos)
        {
            logo.gameObject.SetActive(false);
        }
        if (logoIndex > -1)
            Logos[logoIndex].gameObject.SetActive(true);
        logoIndex++;
        if (logoIndex > Logos.Count - 1)
        {
            logoIndex = -1;
        }
    }

}
