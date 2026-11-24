using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGameplayCamera : MonoBehaviour
{
    public GameObject camHandler;
    public GameObject camNoise;

    public Vector3 LocalNoiseAmp;
    public Vector3 LocalNoiseFrq;

    public Vector3 LocalMiqroNoiseAmp;
    public Vector3 LocalMiqroNoiseFrq;

    public Vector2 LookAtNoiseAmp;
    public Vector2 LookAtNoiseFrq;
    Vector3 lookAtDriftDir;
    Vector3 lookAtDriftDirLerp;

    public Camera cam;
    public float camDistance = 0.8f;
    public float camHeight = 1.4f;
    public float camHeightSpeedMod = 1f;
    public AnimationCurve camHeightSpeedModCurve;
    float currentCamHeight;

    Vector3 followerPos;
    Vector3 leftPos = new Vector3(-1f, 0f, 0f);
    Vector3 rightPos = new Vector3(1f, 0f, 0f);
    Vector3 gotoPos;
    float camLerpVal = 0.5f;
    float camLerpDes = 0.5f;
    public AnimationCurve smoothSidetoSide;

    Vector3 lookForwardLerp;

    public bool playerDied = false;
    float posLerp;

    public AnimationCurve fovCurve;
    public AudioSource windSound;
    public LongBoardControls LongBoardControls;
    
    void Start()
    {
        currentCamHeight = camHeight;
    }
    
    void FixedUpdate()
    {
        if (playerDied) return;

        windSound.pitch = 1.2f + (LongBoardControls.speed0to1Probable * 0.6f);
        windSound.volume = 0.3f + (LongBoardControls.speed0to1Probable * 0.15f);

        float turnVal = LongBoardControls.driftVal * Mathf.Sign(LongBoardControls.dirDot);

        followerPos = LongBoardControls.transform.position + (-LongBoardControls.rb.linearVelocity.normalized * camDistance + (Vector3.up* (currentCamHeight + (camHeightSpeedMod * camHeightSpeedModCurve.Evaluate(LongBoardControls.speed0to1Probable)))));

        cam.fieldOfView = 60f + (fovCurve.Evaluate(LongBoardControls.speed0to1Probable) * 20f);

        if (LongBoardControls.doOneEighty)
        {
            posLerp = 1f;
            lookForwardLerp = Vector3.Lerp(lookForwardLerp, Vector3.zero, Time.fixedDeltaTime * 2f);
        }
        else
        {
            lookAtDriftDir = new Vector3(((Mathf.PerlinNoise(1.84f + Time.time * LookAtNoiseFrq.x, 0f) - 0.5f) * LookAtNoiseAmp.x), ((Mathf.PerlinNoise(0f, 2.51f + Time.time * LookAtNoiseFrq.y) - 0.5f) * LookAtNoiseAmp.y), 0f);
            if (!LongBoardControls.grounded)
            {
                posLerp = Mathf.Lerp(posLerp, 0.4f, Time.fixedDeltaTime);
                camLerpVal = LongBoardControls.lStickInputLerped.x * 0.5f + 0.5f;
                currentCamHeight = Mathf.Lerp(currentCamHeight, 0.8f, Time.fixedDeltaTime * 2f);
                lookForwardLerp = Vector3.Lerp(lookForwardLerp, Vector3.zero, Time.fixedDeltaTime * 2f);
            }
            else
            {
                posLerp = Mathf.Clamp((LongBoardControls.dirDotABS - 0.5f) * 2f, 0.7f, 1f);
                lookForwardLerp = Vector3.Lerp(lookForwardLerp, LongBoardControls.transform.forward * (Mathf.Sign(LongBoardControls.dirDot) * 10f), Time.fixedDeltaTime);
                currentCamHeight = Mathf.Lerp(currentCamHeight, camHeight, Time.fixedDeltaTime);
                posLerp = Mathf.Lerp(posLerp, 1f, Time.fixedDeltaTime * 0.5f);
                camLerpVal = -Mathf.Clamp(turnVal * 20f, -1f, 1f) * 0.5f+0.5f;// smoothSidetoSide.Evaluate(d * 0.5f + 0.5f);
                lookAtDriftDir += -transform.right * (turnVal * 0.6f);
                lookAtDriftDirLerp = Vector3.Lerp(lookAtDriftDirLerp, lookAtDriftDir, Time.fixedDeltaTime * 5f);
            }
            camLerpDes = Mathf.MoveTowards(camLerpDes, camLerpVal, Time.fixedDeltaTime * (0.4f - (Mathf.Abs(LongBoardControls.lStickInputLerped.x) * 0.2f)));
            camHandler.transform.localPosition = Vector3.Lerp(leftPos, rightPos, smoothSidetoSide.Evaluate(camLerpDes));
            lookAtDriftDir += -transform.right * (turnVal * 0.6f);
        }
        
        transform.position = Vector3.Lerp(transform.position, followerPos, Time.fixedDeltaTime * 12f * posLerp);
        transform.LookAt(LongBoardControls.transform.position + Vector3.up + lookAtDriftDirLerp + lookForwardLerp);

        Vector3 noise = new Vector3((Mathf.PerlinNoise(0.34f + Time.time * LocalMiqroNoiseFrq.x, 0f) - 0.5f) * LocalMiqroNoiseAmp.x,
                                         (Mathf.PerlinNoise(0.25f + Time.time * LocalMiqroNoiseFrq.y, 0f) - 0.5f) * LocalMiqroNoiseAmp.y,
                                         (Mathf.PerlinNoise(0.12f + Time.time * LocalMiqroNoiseFrq.z, 0f) - 0.5f) * LocalMiqroNoiseAmp.z);

        camNoise.transform.localPosition = new Vector3((Mathf.PerlinNoise(0.34f + Time.time * LocalNoiseFrq.x, 0f) - 0.5f) * LocalNoiseAmp.x,
                                         (Mathf.PerlinNoise(0.25f + Time.time * LocalNoiseFrq.y, 0f) - 0.5f) * LocalNoiseAmp.y,
                                         (Mathf.PerlinNoise(0.12f + Time.time * LocalNoiseFrq.z, 0f) - 0.5f) * LocalNoiseAmp.z)
                                           + noise;
    }
}
