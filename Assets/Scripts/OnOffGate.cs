using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffGate : MonoBehaviour
{
    public bool HeadGate = true;
    public GameObject gate;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Mathf.Sign(GameManager.Instance.longBoardCTRL.dirDot) < 0f)
        {
            if(gate.activeSelf != HeadGate)
            {
                gate.SetActive(HeadGate);
            }
        }
        else
        {
            if (gate.activeSelf == HeadGate)
            {
                gate.SetActive(!HeadGate);
            }
        }
    }
}
