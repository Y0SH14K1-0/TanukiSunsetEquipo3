using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouletteTicker : MonoBehaviour
{
    public int piecesInRoulette = 6;
    public float minAngularVelocity = 10f;
    public float slowAngularDrag = 1f;
    public float fastAngularDrag = 0.1f;
    public float tickAngularDrag = 10f;
    public float spinTorqueMin = 3000f;
    public float spinTorqueMax = 4000f;
    public float minVelocityToWin = 5f;

    public List<int> winnings;
    public Animator triangleAnimator;

    private int rotationLastTicked;
    private AudioSource audioSource;
    private Rigidbody2D rb2D;
    private bool hasStopped = true;
    private float hasStoppedCooldown = 0f;
    private Action rouletteStoppedFeedback;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    public void ActivateRoulette(Action rouletteStopped = null)
    {
        if (rouletteStopped != null) rouletteStoppedFeedback = rouletteStopped;

        hasStopped = false;
        hasStoppedCooldown = 2f;
        rb2D.AddTorque(UnityEngine.Random.Range(spinTorqueMin, spinTorqueMax));
    }

    void RouletteStopped()
    {
        hasStopped = true;

        int currentDegree = Mathf.FloorToInt(transform.rotation.eulerAngles.z);
        int currentSegmentNumber = Mathf.FloorToInt((float)currentDegree/(360f/(float)piecesInRoulette));
        GameManager.Instance.scoreSystem.InstantTrick(winnings[currentSegmentNumber],LocalizationManager.GetLocalizedUIString("ui-key-bonus"));// "Bonus");
        
        if (rouletteStoppedFeedback != null) rouletteStoppedFeedback();
    }

    void FixedUpdate()
    {   
        CalculateDrag();
        CalculateTickSoundAndAnimation();
        CalculateRouletteStop();
    }

    void CalculateDrag()
    {
        if (rb2D.angularVelocity < minAngularVelocity)
        {
            rb2D.angularDamping = slowAngularDrag;
        } else {
            rb2D.angularDamping = fastAngularDrag;
        }
    }

    void CalculateRouletteStop()
    {
        hasStoppedCooldown -= Time.fixedDeltaTime;
        if (rb2D.angularVelocity <= minVelocityToWin && !hasStopped && hasStoppedCooldown <= 0f)
        {
            RouletteStopped();
        }
    }

    void CalculateTickSoundAndAnimation()
    {
        if (hasStopped) return;
        
        int currentDegree = Mathf.RoundToInt(transform.rotation.eulerAngles.z);
        int currentSegmentNumber = currentDegree/Mathf.RoundToInt(360f/(float)piecesInRoulette);

        bool shouldTick = (currentSegmentNumber != rotationLastTicked);
        
        if (shouldTick)
        {
            audioSource.Play();
            triangleAnimator.SetTrigger("Tick");
            rotationLastTicked = currentSegmentNumber;
            rb2D.angularDamping = tickAngularDrag;
        }
    }
}
