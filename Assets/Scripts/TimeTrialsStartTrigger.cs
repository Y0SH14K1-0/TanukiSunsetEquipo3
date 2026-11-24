using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialsStartTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //if (GameManager.Instance.timeTrialsTimer != null) GameManager.Instance.timeTrialsTimer.StartTimer();
        }
    }
}
