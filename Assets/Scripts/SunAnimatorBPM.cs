using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

public class SunAnimatorBPM : MonoBehaviour
{
    private Animator anim;
    public Renderer rnd;

    public void StartGroovin(float BPM)
    {
        if (anim == null) anim = GetComponent<Animator>();
        anim.SetFloat("speedMultiplier", BPM * 0.1f);
        anim.SetTrigger("restart");
        if (rnd != null)
        {
            rnd.material.SetFloat("_BPM", BPM);
        }
    }

    public void StopGroovin()
    {
        if (anim == null) anim = GetComponent<Animator>();
        anim.SetFloat("speedMultiplier", 0f);
        if (rnd != null)
        {
            rnd.material.SetFloat("_BPM", 20f);
        }
    }
}
