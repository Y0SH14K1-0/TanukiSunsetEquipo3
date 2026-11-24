using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Rigidbody RB;

    public float forwardForce = 500f;
    public float steerAmount = 10f;

    public WheelCollider FL;
    public WheelCollider FR;
    public WheelCollider BL;
    public WheelCollider BR;

    Vector3 slopeDir;
    // Start is called before the first frame update
    void Start()
    {
        RB.linearVelocity = transform.forward * 5f;
    }

    // Update is called once per frame
    void Update()
    {
        FL.steerAngle = RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL) * steerAmount;
        FR.steerAngle = RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL) * steerAmount;
        BL.steerAngle = RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL) * -steerAmount;
        BR.steerAngle = RewindInput.Instance.GetAxis(RewindInputActions.HORIZONTAL) * -steerAmount;

        RaycastHit hit;
        if(Physics.Raycast(transform.position,Vector3.down,out hit,2f))
        {
           slopeDir = Vector3.Cross(-Vector3.Cross(hit.normal, Vector3.up), hit.normal).normalized;
        }


        Debug.DrawLine(transform.position, transform.position + slopeDir, Color.red);
    }

    private void FixedUpdate()
    {
        RB.AddForce(slopeDir * Time.fixedDeltaTime * forwardForce);
    }
}
