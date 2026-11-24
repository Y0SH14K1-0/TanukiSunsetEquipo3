using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectablesHUD : MonoBehaviour
{
    public int pointsToCompleteWheel = 100;

    public Image wheelFillImage;
    public RouletteTicker rouletteTicker;
    public RandomAudioSource yumAudioSource;
    public RandomAudioSource doritoAudioSource;
    public GameObject beepPrefab;
    public Transform beepParentTransform;
    public AudioClip beepAudioClip;
    public float yumSFXCooldown = 3f;
    public TanukiBitsHUD tanukiBitsHUD;
    public bool enableWheelMechanic = true;
    public int bitsToMunchToBurp;

    public AudioSource munchAudioSource;
    public AudioSource clinkAudioSource;
    public AudioClip bigMunchAudioClip;
    public AudioClip burpMunchAudioClip;
    public AudioClip clinkAudioClip;
    public List<AudioClip> mutedMunchAudioClips;

    private Animator _animator;
    private int _currentPoints = 0;
    private bool _activatedRoulette = false;

    private float _yumSFXCooldown = 0f;
    private bool _yumSFXWillActivate = false;
    private int _chanceOfYumOneInHowMany = 100;
    private int _numberOfBeeps = 0;
    private float _beepComboCooldown;
    private float _munchCooldown;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _currentPoints = 0;
        wheelFillImage.fillAmount = 0f;

        foreach (Animator animator in GetComponentsInChildren<Animator>())
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }

    void Update()
    {
        _beepComboCooldown -= Time.deltaTime;
        if (_beepComboCooldown <= 0f)
        {
            _numberOfBeeps = 0;
        }
                
        _yumSFXCooldown -= Time.deltaTime;
        if (_yumSFXCooldown <= 0f && _yumSFXWillActivate)
        {
            _yumSFXWillActivate = false;
            _yumSFXCooldown = yumSFXCooldown;
            if (Random.Range(0, _chanceOfYumOneInHowMany) == 0)
            {
                yumAudioSource.PlayRandom();
            }
        }

        _munchCooldown += Time.deltaTime;

        if (_munchCooldown > 0.7f && bitCount >= bitsToMunchToBurp)
        {
            GameManager.Instance.scoreSystem.InstantTrick(10f * (float)bitCount,LocalizationManager.GetLocalizedUIString("ui-key-munch-king"));//"Munch King"
            bitCount = 0;
            munchAudioSource.volume = 1f;
            munchAudioSource.pitch = 0.8f + (Mathf.Clamp(((float)(bitsToMunchToBurp*2) - (float)bitCount) / 10f, 0f, 1f) * 0.8f);
            munchAudioSource.clip = burpMunchAudioClip;
            munchAudioSource.Play();
        }
    }

    public void CollectPoints(int bitAmount = 1)
    {
        tanukiBitsHUD.CollectBit(bitAmount);
        //doritoAudioSource.PlayRandom();
        MunchSound();

        if (!enableWheelMechanic) return;
        Beep();
        _yumSFXCooldown = yumSFXCooldown;
        _yumSFXWillActivate = true;
        _currentPoints++;
        if (_currentPoints >= pointsToCompleteWheel && !_activatedRoulette)
        {
            StartCoroutine(ActivateRoulette());
            return;
        }
        _animator.SetTrigger("CollectPoints");
    }

    private float munchPitch = 0.9f;
    private int bitCount = 0;

    void MunchSound()
    {
        clinkAudioSource.pitch = Random.Range(1.2f, 1.8f);
        clinkAudioSource.volume = Random.Range(0.1f, 0.4f);
        clinkAudioSource.PlayOneShot(clinkAudioClip);

        if (munchAudioSource.isPlaying) return;

        if (_munchCooldown >= 0.7f)
        {
            bitCount = 1;
            munchPitch = 0.9f;
            munchAudioSource.pitch = Random.Range(0.8f, 1.2f);
            munchAudioSource.clip = bigMunchAudioClip;
        } else
        {
            bitCount += 1;
            munchPitch += Random.Range(0.005f, 0.02f);
            if (munchPitch > 1.2f) munchPitch = 1.2f;
            munchAudioSource.volume = Random.Range(0.6f, 1f);
            munchAudioSource.pitch = munchPitch;
            munchAudioSource.clip = mutedMunchAudioClips[Random.Range(0,mutedMunchAudioClips.Count)];  
        }

        munchAudioSource.Play();
        _munchCooldown = 0f;
    }

    IEnumerator ActivateRoulette()
    {
        _activatedRoulette = true;
        for (float f = 0f; f <= 1f; f += Time.deltaTime/0.2f)
        {
            wheelFillImage.fillAmount = Mathf.Lerp(wheelFillImage.fillAmount, 1f, Mathf.SmoothStep(0f, 1f, f));
            yield return null;
        }
        _animator.SetBool("OnRoulette", true);
        yield return new WaitForSeconds(2f);
        rouletteTicker.ActivateRoulette(RouletteStoppedCallback);
        yield return new WaitForSeconds(2f);
        _currentPoints = 0;
        wheelFillImage.fillAmount = 0f;
        _activatedRoulette = false;
    }

    void RouletteStoppedCallback()
    {
         _animator.SetBool("OnRoulette", false);
    }
    
    public void LerpWheelFill()
    {
        StartCoroutine(C_LerpWheelFill());
    }

    IEnumerator C_LerpWheelFill()
    {
        float fillDuration = 0.2f;
        float currentFill = wheelFillImage.fillAmount;
        float targetFill = (float)_currentPoints/(float)pointsToCompleteWheel;
        for (float f = 0f; f <= 1f; f += Time.deltaTime/fillDuration)
        {
            wheelFillImage.fillAmount = Mathf.Lerp(currentFill, targetFill, Mathf.SmoothStep(0f, 1f, f));
            yield return null;
        }
        wheelFillImage.fillAmount = targetFill;
    }

    public void Beep()
    {
        _numberOfBeeps++;
        _beepComboCooldown = 0.5f;
        GameObject beep = GameObject.Instantiate(beepPrefab, beepParentTransform);
        AudioSource beepAudioSource = beep.GetComponent<AudioSource>();
        if (_numberOfBeeps >= 12) _numberOfBeeps = 12;
        beepAudioSource.pitch = Mathf.Pow(1.05946f, (float)_numberOfBeeps-6f);
        beepAudioSource.volume = Mathf.Lerp(0.25f, 0.5f, (float)_numberOfBeeps / 12f);
        beepAudioSource.PlayOneShot(beepAudioClip);
    }
}
