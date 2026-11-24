using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Camera Cam;
    public float distance = 3f;
    float fovDist;
    public float height = 1f;
    public float LookAtHeight = 1.8f;
    public Vector2 noiseAMP = new Vector2(0.6f,0.3f);
    public float noiseFreq = 10f;
    Vector3 posLerp;
    Vector3 targetPos;
    Vector2 lStickInputLerped;
    Vector3 camNoise;
    float turnTilt;
    float turnTiltLerped;
    public AnimationCurve noiseCRV;
    Vector3 camLookAtPos;
    float steerDirLerp;

    public float lookAtTurnAmount = 2f;
    float lookAtTurnCurrent;
    float lookAtTurnLerp;

    public bool PlayerDied = false;
    // Start is called before the first frame update
    void Start()
    {
        lookAtTurnCurrent = lookAtTurnAmount;
    }

    private void FixedUpdate()
    {
        lStickInputLerped = Vector2.Lerp(lStickInputLerped, new Vector2(RewindInput.Instance.GetAxisRaw(RewindInputActions.R_HORIZONTAL), RewindInput.Instance.GetAxisRaw(RewindInputActions.R_VERTICAL)), Time.deltaTime * 5f);
        //lStickInputLerped *= GameManager.Instance.longBoardCTRL.dirDotABS;

        turnTilt = Vector3.Dot(transform.right * Mathf.Sign(GameManager.Instance.longBoardCTRL.dirDot), GameManager.Instance.longBoardCTRL.transform.forward) * GameManager.Instance.longBoardCTRL.dirDotABS * Mathf.Clamp01(GameManager.Instance.longBoardCTRL.speed0to1 * 2f);
        turnTiltLerped = Mathf.Lerp(turnTiltLerped, turnTilt, Time.deltaTime * 1f);

        fovDist = distance - (GameManager.Instance.longBoardCTRL.speed0to1 * 1f);

        if (GameManager.Instance.longBoardCTRL.doOneEighty || !GameManager.Instance.longBoardCTRL.grounded)
            lookAtTurnCurrent = 0f;
        else
            lookAtTurnCurrent = lookAtTurnAmount;
        lookAtTurnLerp = Mathf.Lerp(lookAtTurnLerp, lookAtTurnCurrent, Time.deltaTime * 5f);

        camNoise = new Vector3((Mathf.PerlinNoise(Time.time * noiseFreq, 5f) - 0.5f) * noiseAMP.x, (Mathf.PerlinNoise(2.5f, Time.time * noiseFreq) - 0.5f) * noiseAMP.y, 0f) * noiseCRV.Evaluate(GameManager.Instance.longBoardCTRL.speed0to1);
        if (GameManager.Instance.longBoardCTRL.rb.linearVelocity.magnitude > 3f)
        {
            targetPos = GameManager.Instance.longBoardCTRL.transform.position - (GameManager.Instance.longBoardCTRL.rb.linearVelocity.normalized * fovDist) + (Vector3.up * (height - Mathf.Abs(turnTiltLerped*3.4f)));
            targetPos += (Vector3.up * lStickInputLerped.y) + (-transform.right * lStickInputLerped.x * 2f);
            targetPos += (transform.right * camNoise.x) + (transform.up * camNoise.y);
            targetPos += (transform.right * -turnTiltLerped * 6f);
        }

        if (!PlayerDied)
        {
            steerDirLerp = Mathf.Lerp(steerDirLerp, -GameManager.Instance.longBoardCTRL.driftVal * Mathf.Sign(GameManager.Instance.longBoardCTRL.dirDot), Time.deltaTime * 2f);
            camLookAtPos = GameManager.Instance.longBoardCTRL.transform.position + (Vector3.up * LookAtHeight) + (transform.right * lookAtTurnLerp * steerDirLerp * GameManager.Instance.longBoardCTRL.dirDotABS * GameManager.Instance.longBoardCTRL.speed0to1);
            transform.LookAt(camLookAtPos);
            Cam.fieldOfView = 70f + (GameManager.Instance.longBoardCTRL.speed0to1 * 30f);
            posLerp = Vector3.Lerp(posLerp, targetPos, Time.fixedDeltaTime * 12f);
            transform.position = posLerp;
        }
    }
}
