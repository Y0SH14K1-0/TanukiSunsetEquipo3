using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundCamera : MonoBehaviour
{
    private Camera cam;
    public AnimationCurve fovCurve;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        cam.fieldOfView = Mathf.Clamp(60f +(40f * fovCurve.Evaluate( TanukiTestRoot.Instance.longBoardControls.speed0to1Probable)),60f,100f);
        //Debug.Log("speed 0 to 1  - " + GameManager.Instance.longBoardCTRL.speed0to1Probable + " velocity - " + GameManager.Instance.longBoardCTRL.RB.velocity.magnitude);
    }
}
