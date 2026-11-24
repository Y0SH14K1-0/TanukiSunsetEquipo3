using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreCameraNoiseMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 LocalNoiseAmp;
    public Vector3 LocalNoiseFrq;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(Mathf.PerlinNoise(0.34f + Time.time * LocalNoiseFrq.x, 0f) * LocalNoiseAmp.x,
                                         Mathf.PerlinNoise(0.25f + Time.time * LocalNoiseFrq.y, 0f) * LocalNoiseAmp.y,
                                         Mathf.PerlinNoise(0.12f + Time.time * LocalNoiseFrq.z, 0f) * LocalNoiseAmp.z);
    }
}
