using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTanukiTrigger : MonoBehaviour
{
    public float amout = 1f;
    // Start is called before the first frame update

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameManager.Instance.longBoardCTRL.crabWiggle = amout;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameManager.Instance.longBoardCTRL.crabWiggle = 0f;
        }
    }
}
