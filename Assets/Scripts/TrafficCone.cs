using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficCone : MonoBehaviour
{
    Rigidbody RB;
    public AudioSource audioSource;
    public bool canSpeedThrough = false;
    public float slowDownAmount = 0.35f;
    
    void Start()
    {
        RB = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            RB.isKinematic = false;
            RB.linearVelocity = ((transform.position - TanukiTestRoot.Instance.longBoardControls.transform.position).normalized*20f) + (Vector3.up*5f);
            RB.AddTorque(new Vector3(Random.Range(-80f, 80f), Random.Range(-80f, 80f), Random.Range(-80f, 80f)),ForceMode.VelocityChange);

            if(canSpeedThrough)
            {
                if(!TanukiTestRoot.Instance.longBoardControls.unlockSpeedLimit)
                {
                    TanukiTestRoot.Instance.longBoardControls.GentleSlowdown(slowDownAmount);
                }
            }
            else
            {
                TanukiTestRoot.Instance.longBoardControls.GentleSlowdown(slowDownAmount);
            }

            audioSource.pitch = Random.Range(0.7f, 1.3f);
            audioSource.Play();
        }
    }
}
