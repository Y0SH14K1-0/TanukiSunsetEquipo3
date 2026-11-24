using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlipBoardUnderGate : MonoBehaviour
{
    public BoxCollider col;
    // Start is called before the first frame update

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if(!GameManager.Instance.longBoardCTRL.tanukiIsFlipping)
            {
                col.enabled = false;
                GameManager.Instance.longBoardCTRL.GentleSlowdown(0.8f);
            }
        }
    }
}
