using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueBitGroup : MonoBehaviour
{
    void Update()
    {
        if(transform.childCount == 0)
        {
            TanukiTestRoot.Instance.longBoardControls.DoRevertBitCombo();
            TanukiTestRoot.Instance.longBoardControls.GentleSpeedup(0.8f);
            Destroy(gameObject);
        }
    }
}
