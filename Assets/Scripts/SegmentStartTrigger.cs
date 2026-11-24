using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentStartTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if (TanukiTestRoot.Instance.RoadGenerator.isActiveAndEnabled)
            {
                TanukiTestRoot.Instance.RoadGenerator.IncreaseStartTriggerCount();
                TanukiTestRoot.Instance.RoadGenerator.GenerateNextSegment();
            }
            gameObject.SetActive(false);
        }
        
    }
}
