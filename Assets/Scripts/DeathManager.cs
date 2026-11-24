using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public LongBoardControls LBC;
    public GameObject raccoon;
    float fallCountDownTime = 1f;
    float fallCounter = 0f;
    bool PlayerDied = false;
    public GameObject loseBitsEffect;
    public int bitsLostWhenDead = 5;

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(LBC.fTracksPoint.position, -transform.up, 50f) || Physics.Raycast(LBC.bTracksPoint.position, -transform.up, 50f))
        {
            if(fallCounter != 0f)
                fallCounter = 0f;
        }
        else
        {
            fallCounter += Time.deltaTime;
            if(fallCounter > fallCountDownTime)
            {
                Death("Fall counter exceeded fall countdown time.");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        //Debug.Log(collision.relativeVelocity.magnitude);

        if (collision.gameObject.tag != "Road" && collision.gameObject.tag != "Ramp")// && LBC.speed0to1 > 0.4f)
        {
            Death("Collision with GameObject with RigidBody: '" + collision.gameObject.name + "'");
        }
    }

    void Death(string deathReason)
    {

        if (!PlayerDied)
        {
            Instantiate(loseBitsEffect, raccoon.transform.position, Quaternion.identity);
            PlayerDied = true;
            LBC.isAlive = false;
            LBC.TakingInput = false;
            raccoon.transform.parent = null;
            raccoon.AddComponent<Rigidbody>();
            raccoon.GetComponent<Rigidbody>().linearVelocity = LBC.rb.linearVelocity;
            LBC.rb.linearVelocity = -LBC.rb.linearVelocity * 0.5f;
            LBC.rb.AddForceAtPosition(transform.up * 300f, LBC.bTracksPoint.transform.position, ForceMode.Impulse);

            Rewired.ReInput.players.GetPlayer(0).StopVibration();
            AudioService.Instance.PlayerDied();
        }
    }
}
