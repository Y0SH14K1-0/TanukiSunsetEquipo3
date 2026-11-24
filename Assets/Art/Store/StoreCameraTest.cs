using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreCameraTest : MonoBehaviour
{
    public Animator anim;
    int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))//Character customization
        {
            index = 1;
            SetIndex(index);
        }
            
        if (Input.GetKeyDown(KeyCode.Alpha2))//Endless
        {
            index = 2;
            SetIndex(index);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))//World map
        {
            index = 3;
            SetIndex(index);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))//Options
        {
            index = 4;
            SetIndex(index);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))//Board Customization
        {
            index = 5;
            SetIndex(index);
        }
        if (Input.GetKeyDown(KeyCode.Escape))//Back
        {
            index = 0;
            SetIndex(index);
        }


        if (Input.GetKeyDown(KeyCode.RightArrow))//Right
        {
            if (index < 5)
            {
                index++;
                SetIndex(index);
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))//Left
        {
            if (index >= 1)
            {
                index--;
                SetIndex(index);
            }
        }
    }

    void SetIndex(int x)
    {
        anim.SetInteger("Index", x);
    }
}
