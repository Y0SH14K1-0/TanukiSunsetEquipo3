using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineWaveBob : MonoBehaviour
{
    // Start is called before the first frame update

    public float AMP = 1f;
    public float FRQ = 1f;
    public float offset = 0f;
    private float startY;

    void Start()
    {
        startY = transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, startY + (Mathf.Sin((Time.time + offset)* FRQ)*AMP), transform.localPosition.z);
    }
}
