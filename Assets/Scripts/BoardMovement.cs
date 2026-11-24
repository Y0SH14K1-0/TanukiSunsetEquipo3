using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardMovement : MonoBehaviour
{
    public Rigidbody RB;
    Vector2 lStickInput;
    public Transform fTracksPoint;
    public Transform bTracksPoint;
    bool grounded = false;
    public float maxSpeed = 10f;
    public float acclMul = 25f;
    public float bVelocity;
    float tilt;
    Vector3 frdNoY;
    Vector3 vLerp;
    float speed0to1;
    public float wheelsDamping = 0.01f;
    public float turnPower = 5f;
    public AnimationCurve tractionCurve;
    public AnimationCurve slideCurve;
    float boardDirDot;
    bool goofy = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        lStickInput = new Vector2(RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL), RewindInput.Instance.GetAxis(RewindInputActions.VERTICAL));

        if (Physics.Raycast(fTracksPoint.position,Vector3.down,0.3f) && Physics.Raycast(bTracksPoint.position, Vector3.down, 0.3f))
        {
            grounded = true;

            tilt = bTracksPoint.position.y - fTracksPoint.position.y;

            boardDirDot = Vector3.Dot(transform.forward, RB.linearVelocity.normalized);
            
            if(bVelocity > -maxSpeed && bVelocity < maxSpeed)
            {
                
                bVelocity += tilt * Time.deltaTime * acclMul;
            }
            bVelocity -= Mathf.Sign(bVelocity) * Time.deltaTime * wheelsDamping;
            bVelocity -= Mathf.Sign(bVelocity) * Time.deltaTime * (1f - tractionCurve.Evaluate(Mathf.Abs(boardDirDot))) * 0.0005f;

            //bVelocity *= Mathf.Sign(boardDirDot);

            //bVelocity = Mathf.Lerp(0f, bVelocity, tractionCurve.Evaluate((boardDirDot)));

            if (Mathf.Sign(fTracksPoint.position.y - bTracksPoint.position.y) <0f && !goofy)
            {
                goofy = true;
                bVelocity *= -1f;
            }
            else if(Mathf.Sign(fTracksPoint.position.y - bTracksPoint.position.y) > 0f && goofy)
            {
                goofy = false;
                bVelocity *= 1f;
            }

            frdNoY = new Vector3(transform.forward.x * bVelocity, RB.linearVelocity.y, transform.forward.z * bVelocity);
            speed0to1 = Mathf.Abs(bVelocity) / maxSpeed;

            vLerp = Vector3.Lerp(frdNoY, RB.linearVelocity, slideCurve.Evaluate(speed0to1) *0.98f);

        }
        else
        {
            grounded = false;
        }

    }

    private void FixedUpdate()
    {
        if(grounded)
        {
            RB.linearVelocity = vLerp;

            RB.MoveRotation(Quaternion.LookRotation(transform.forward + (transform.right * lStickInput.x * Time.fixedDeltaTime * turnPower)));
        }
    }
}
