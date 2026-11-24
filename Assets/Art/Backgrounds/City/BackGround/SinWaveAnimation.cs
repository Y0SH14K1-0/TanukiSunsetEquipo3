using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinWaveAnimation : MonoBehaviour
{
    public Vector3 animAxis;
    public float animSpeed = 1f;
    Vector3 startRot;
    // Start is called before the first frame update
    void Start()
    {
        startRot = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles = startRot + (animAxis * Mathf.Sin(Time.time * animSpeed));
    }
}
