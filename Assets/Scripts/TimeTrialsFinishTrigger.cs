using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrialsFinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            StartCoroutine(C_FinishTimeTrialsTimer());
        }
    }

    private IEnumerator C_FinishTimeTrialsTimer()
    {

        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = Time.timeScale / 50f;

        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 0f;
    }
}
