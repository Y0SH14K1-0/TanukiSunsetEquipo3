using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TanukiBit : MonoBehaviour
{
    public bool collectable = false;
    public bool EvilBit = false;

    public GameObject GoodBit;
    public GameObject BadBit;
    public GameObject PickUpEffect;
    public GameObject BadPickUpEffect;

    float slowDownAmount = 0.45f;

    private void Start()
    {
        if (GoodBit == null || BadBit == null) return;
        if (!EvilBit)
        {
            GoodBit.SetActive(true);
            BadBit.SetActive(false);
        }
        else
        {
            GoodBit.SetActive(false);
            BadBit.SetActive(true);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (collectable)
            {
                if(!EvilBit)
                {
                    Destroy(gameObject);
                    if (PickUpEffect != null)
                        Instantiate(PickUpEffect, transform.position, transform.rotation);
                    TanukiTestRoot.Instance.longBoardControls.GentleSpeedup(0.3f);
                }
                else
                {
                    Destroy(gameObject);
                    if (BadPickUpEffect != null)
                        Instantiate(BadPickUpEffect, transform.position, transform.rotation);
                    TanukiTestRoot.Instance.longBoardControls.GentleSlowdown(slowDownAmount);
                }
            }
        }
    }
}
