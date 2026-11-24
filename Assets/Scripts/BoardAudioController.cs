using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardAudioController : MonoBehaviour
{
    public LongBoardControls LBC;
    public AudioSource rideSound;
    public AudioSource driftSound;
    public AudioSource dryNoiseSound;
    public AudioSource wetNoiseSound;

    // Update is called once per frame
    void Update()
    {
        if(LBC.grounded)
        {
            float rideVolume = Mathf.Clamp01(LBC.speed0to1 * 2f) * 0.8f * LBC.dirDotABS;
            rideSound.volume = rideVolume;
            dryNoiseSound.volume = rideVolume;
            wetNoiseSound.volume = rideVolume;

            float ridePitch = 1f + (LBC.speed0to1 * 0.4f);
            rideSound.pitch = ridePitch;
            dryNoiseSound.pitch = ridePitch;
            wetNoiseSound.pitch = ridePitch;

            driftSound.volume = Mathf.Clamp01(LBC.speed0to1 * 2f) * 1f * (1f-LBC.dirDotABS);
            driftSound.pitch = 1.2f + (LBC.speed0to1 * 0.6f);
        }
        else
        {
            rideSound.volume = 0f;
            driftSound.volume = 0f;
            dryNoiseSound.volume = 0f;
            wetNoiseSound.volume = 0f;
        }
        
    }
}
