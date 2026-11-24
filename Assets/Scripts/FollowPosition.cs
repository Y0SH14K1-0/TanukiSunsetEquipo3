using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    public Transform thing;
    public bool followPosition = true;
    public bool followRotation = false;
    Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        offset =   transform.position - thing.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (followPosition)
            transform.position = thing.position + offset;
        if (followRotation)
            transform.rotation = thing.rotation;
    }
}
