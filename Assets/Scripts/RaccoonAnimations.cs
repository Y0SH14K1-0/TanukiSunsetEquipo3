using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaccoonAnimations : MonoBehaviour
{
    public LongBoardControls LBC;
    public Animator anim;
    float handVal;
    public ParticleSystem leftHandSparks;
    public ParticleSystem rightHandSparks;

    public ParticleSystem bitRevertEffect;
    public AudioSource bitRevertSoundEffect;

    bool flip = false;
    bool playerDead = false;

    float gofastLerp = 0f;
    float steerMul = 0.2f;
    float steerMulLerp = 0.2f;
    float steerLerp = 0f;
    float S;

    public float groovingBPM = 60f;
    float MusicBD = 1f;
    public AnimationCurve groovinDBcurve;

    public ParticleSystem ImpactEffect;
    public AudioSource ImpactEffectSound;
    public AudioSource tanukiAc;
    public AudioClip[] TanukiSounds;
    // Start is called before the first frame update
    void Start()
    {
        AudioService.Instance.audioSourceLoudnessUtility.OnUpdateSongIntensity += OnUpdateSongIntensity;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0.1f)
        {

            if (LBC.grounded)
            {
                if (flip)
                    flip = false;

                if (!anim.GetBool("grounded"))
                {
                    anim.SetBool("grounded", true);
                    anim.SetTrigger("land");
                    anim.SetBool("grab", false);
                }

                if (LBC.speed0to1 > 0.1)
                {
                    float H = Mathf.Clamp((LBC.driftVal * 2f) + (-LBC.steerDir * 0.2f), -1f, 1f);

                    if (gofastLerp > 0.8f)
                        steerMul = 0.2f;
                    else
                        steerMul = 1f;

                    S = Mathf.Lerp(S, H * steerMulLerp, Time.deltaTime * 12f);

                    if (LBC.tanukiFliped)
                    {
                        steerLerp = -S;
                        handVal = -H;
                        //Debug.Log("Tanuki fliped");
                    }
                    else
                    {
                        steerLerp = S;
                        handVal = H;
                    }
                    anim.SetFloat("steer", steerLerp);
                }
                anim.SetBool("drifting", LBC.drifting);

                if (LBC.doOneEighty)
                    gofastLerp = Mathf.Lerp(gofastLerp, 0f, Time.deltaTime * 12f);
                else
                    gofastLerp = Mathf.Lerp(gofastLerp, LBC.lStickInputLerped.y, Time.deltaTime * 12f);

                anim.SetFloat("goFast", gofastLerp);
                steerMulLerp = Mathf.Lerp(steerMulLerp, steerMul, Time.deltaTime * 4f);


                if (anim.GetBool("groovin"))
                {
                    if (LBC.drifting)
                    {
                        anim.SetLayerWeight(1, Mathf.Clamp01(1f - (LBC.lStickInputLerped.magnitude * 0.5f) - 0.5f) * groovinDBcurve.Evaluate(MusicBD));
                    }
                    else
                    {
                        anim.SetLayerWeight(1, Mathf.Clamp01(1f - (LBC.lStickInputLerped.magnitude * 0.5f)) * groovinDBcurve.Evaluate(MusicBD));
                    }
                }
            }
            else
            {
                if (anim.GetBool("grounded"))
                    anim.SetBool("grounded", false);
                anim.SetFloat("airRot", LBC.lStickInputLerped.x);
                anim.SetBool("grab", LBC.isGrabbing);

                if (!flip && LBC.doFlip)
                {
                    anim.SetTrigger("flip");
                    flip = true;
                }

                if (anim.GetBool("groovin"))
                {

                    anim.SetLayerWeight(1, 0f);

                }
            }

            if (!LBC.isAlive && !playerDead)
            {
                anim.SetTrigger("KO");
                if (ImpactEffect != null) ImpactEffect.Play();
                playerDead = true;
            }

            if (LBC.grounded && LBC.speed0to1 > 0.3f && !LBC.tanukiIsFlipping)
            {
                if (handVal > 0.86f)
                {
                    if (!leftHandSparks.isPlaying)
                        leftHandSparks.Play();
                }
                else
                {
                    if (leftHandSparks.isPlaying)
                        leftHandSparks.Stop();
                }
                if (handVal < -0.86f)
                {
                    if (!rightHandSparks.isPlaying)
                        rightHandSparks.Play();
                }
                else
                {
                    if (rightHandSparks.isPlaying)
                        rightHandSparks.Stop();
                }
            }
            else
            {
                if (rightHandSparks.isPlaying)
                    rightHandSparks.Stop();

                if (leftHandSparks.isPlaying)
                    leftHandSparks.Stop();
            }

            if (LBC.highFive)
            {
                anim.SetTrigger("highfive");
                LBC.highFive = false;
            }
            if (LBC.revertBitCombo)
            {
                anim.SetTrigger("highfive");
                LBC.revertBitCombo = false;
                if (bitRevertEffect != null) bitRevertEffect.Play();
                if (bitRevertSoundEffect != null) bitRevertSoundEffect.Play();
            }

            anim.SetFloat("movementDirX", -LBC.driftVal);
            anim.SetFloat("movementDirY", LBC.dirDot);
        }
    }

    public void StartGroovin(float BPM)
    {
        anim.SetFloat("groovinMultiplier", BPM*0.028f);
        anim.SetBool("groovin", true);
    }
    public void StopGroovin()
    {
        anim.SetBool("groovin", false);
    }

    void OnUpdateSongIntensity(float intensity)
    {
        MusicBD = Mathf.Clamp01(Mathf.Lerp(MusicBD, intensity*5f,Time.deltaTime*10f));
    }

    public void SetBumpAnimTrigger()
    {
        anim.SetTrigger("bump");
        if (ImpactEffect != null) ImpactEffect.Play();
        if (ImpactEffectSound != null) ImpactEffectSound.Play();
    }
    public void SetTanukiFlipAnimTrigger()
    {
        anim.SetTrigger("tanukiFlip");
        if(tanukiAc != null)
        {
            tanukiAc.clip = TanukiSounds[Random.Range(0, TanukiSounds.Length)];
            tanukiAc.Play();
        }
    }

}
