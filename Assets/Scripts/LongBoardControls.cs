using System.Collections;
using UnityEngine;

public class LongBoardControls : MonoBehaviour
{
    public Rigidbody rb;
    public float myHorizontal, myVertical;
    public bool myDrift;
    [HideInInspector] public bool unlockSpeedLimit = false;
    public bool grounded = true;
    [HideInInspector] public bool drifting;
    [HideInInspector] public bool isGrabbing = false;
    [HideInInspector] public bool doFlip = false;
    [HideInInspector] public bool highFive = false;
    [HideInInspector] public bool revertBitCombo = false;
    [HideInInspector] public float dirDot;
    [HideInInspector] public float dirDotABS;
    [HideInInspector] public float speed0to1;
    [HideInInspector] public float speed0to1Probable;
    [HideInInspector] public float steerDir;
    [HideInInspector] public float driftVal;
    [HideInInspector] public Vector2 lStickInputLerped;
    public bool enableSlowDownFunctionality = true;
    public bool rumbleEnabled = true;
    public bool EnableRevertFunction = true;
    public bool doOneEighty = false;
    public bool TakingInput = true;
    public bool tanukiFliped = false;
    public bool tanukiIsFlipping = false;
    public bool isAlive = true;
    public float accelMul = 20f;
    public float topHighSpeed = 30f;
    public float maxSpeed = 50f;
    public float minSpeed = 10f;
    public float slowDownSpeed = 0.5f;
    public float turnPower = 20f;
    public float turnSlidePower = 20f;
    public float turnAirPower = 60f;
    public float driftRumbleValue = 0.3f;
    public float driftBoostAmount = 30f;
    public float crabWiggle = 0f;
    public float oneEightyRotationSpeed = 240f;
    public Animator anim;
    public AnimationCurve tractionCurve;
    public AnimationCurve driftingCurve;
    public GameObject BL_Wheel_holder;
    public GameObject BR_Wheel_holder;
    public GameObject FL_Wheel_holder;
    public GameObject FR_Wheel_holder;
    public GameObject honkSFXPrefab;
    public GameObject landingEffect;
    public Transform fTracksPoint;
    public Transform bTracksPoint;
    public Transform frontREdgeDetect;
    public Transform frontLEdgeDetect;
    public Transform backREdgeDetect;
    public Transform backLEdgeDetect;
    public ParticleSystem frontREdgePS;
    public ParticleSystem frontLEdgePS;
    public ParticleSystem backREdgePS;
    public ParticleSystem backLEdgePS;
    public ParticleSystem slideDust_Front_PS;
    public ParticleSystem slideDust_Back_PS;
    public ParticleSystem FR_wheel_PS;
    public ParticleSystem BR_wheel_PS;
    public ParticleSystem FL_wheel_PS;
    public ParticleSystem BL_wheel_PS;
    public ParticleSystem flipPS;
    public ParticleSystem GoodDriftEffect;
    public ParticleSystem gainingSpeedEffect;
    public ParticleSystem bitBoostEffect;
    public AudioSource flipSound;
    public RaccoonAnimations tanukiAnimations;

    private bool _slowDown = false;
    private float _brakeAmount = 0f;
    private bool _sliding = false;
    private bool _gainingPoints = false;
    private bool _closeCall = false;
    private bool _overEdge = false;
    private bool _canOneEighty = true;
    private bool _oneEightyStartGoofy = false;
    private bool _ridingFakie = false;
    private bool _fixLandingDir = false;
    private bool _didGrab = false;
    private bool _canGrab = false;
    private bool _canFlip = false;
    private bool _onRamp = false;
    private bool _dontTanukiAtAll = false;
    private bool _wasDrifting = false;
    private float _currentTurnPower;
    private float _slideVal = 0f;
    private float _lSlideVal = 0f;
    private float _tiltVal = 0f;
    private float _airRotDegCounter;
    private float _startOneEightyDir = 0f;
    private float _stickDirClampValue;
    private float _gentleSlowdownCounter = 0f;
    private float _gentleSpeedupCounter = 0f;
    private float _driftSpeedBoostCounter = 0f;
    private float _flipTanukiCounter = 999f;
    private float _tanukiFlipDirection = 1f;
    private float _distanceTraveled = 0f;
    private Vector2 _lStickInput;
    private Vector3 _fVelocity;
    private Vector3 _rampVector;
    
    private void Start()
    {
        rb.centerOfMass -= Vector3.up*0.5f;
        _currentTurnPower = turnPower;
        anim.SetFloat(GrabDir, 1f);
        _distanceTraveled = 0f;
        rumbleEnabled = PlayerPrefs.GetString("EnableRumble", "false") == "true";
        TanukiTestRoot.Instance.InputCheck.onInputHasChanged += OnInputChange;
    }

    private void OnDestroy()
    {
        TanukiTestRoot.Instance.InputCheck.onInputHasChanged -= OnInputChange;
    }

    public void EnableGameplay()
    {
        _dontTanukiAtAll = false;
        rb.useGravity = true;
        TakingInput = true;
        _slideVal = 0.4f;
        _lSlideVal = 0.4f;
        _currentTurnPower = turnPower;
        myDrift = false;
        drifting = false;
        _wasDrifting = false;
        _driftSpeedBoostCounter = 0f;
        _gentleSpeedupCounter = 0f;
        _gentleSlowdownCounter = 0f;
        _gainingPoints = false;
        _canFlip = false;
        rb.linearVelocity = transform.forward * minSpeed;
    }
    public void DisableGameplay()
    {
        _dontTanukiAtAll = true;
        rb.useGravity = false;
        TakingInput = false;
    }

    public float GetDistanceTraveled()
    {
        return _distanceTraveled;
    }

    public void SetCarryOverDistance(float distance)
    {
        _distanceTraveled = distance;
    }

    private void SetRumble(float value)
    {
        if (!rumbleEnabled) return;
        if (TanukiTestRoot.Instance.InputCheck.GetInputType() != InputCheck.InputType.Controller) return;

        //var direction = RewindInput.Instance.GetAxisRaw(RewindInputActions.HORIZONTAL);
        var direction = myHorizontal;

        var leftVibration = value*(0.5f+(direction/2f));
        var rightVibration = value*(0.5f+(-direction/2f));

        Rewired.ReInput.players.GetPlayer(0).SetVibration(0, leftVibration);
        Rewired.ReInput.players.GetPlayer(0).SetVibration(1, rightVibration);
    }

    private float _fixLandingCooldown = 0f;

    private static readonly int Flip = Animator.StringToHash("flip");
    private static readonly int Grab = Animator.StringToHash("grab");
    private static readonly int Steer = Animator.StringToHash("Steer");
    private static readonly int GrabDir = Animator.StringToHash("grabDir");
    
    private void Update()
    {
        BL_Wheel_holder.transform.Rotate(-BL_Wheel_holder.transform.right, Time.deltaTime * 2000f * dirDot * speed0to1, Space.World);
        BR_Wheel_holder.transform.Rotate(BR_Wheel_holder.transform.right, Time.deltaTime * 2000f * dirDot * speed0to1, Space.World);
        FL_Wheel_holder.transform.Rotate(-FL_Wheel_holder.transform.right, Time.deltaTime * 2000f * dirDot * speed0to1, Space.World);
        FR_Wheel_holder.transform.Rotate(FR_Wheel_holder.transform.right, Time.deltaTime * 2000f * dirDot * speed0to1, Space.World);


        ////////////////// 180 spin /////////////////
        if (TakingInput && RewindInput.Instance.GetButtonDown(RewindInputActions.SWITCH) && speed0to1 > 0.4f && !doOneEighty && _canOneEighty && grounded && !_fixLandingDir && Time.timeScale > 0.5f && EnableRevertFunction)
        {
            StartOneEighty();
        }
        if (!_canOneEighty && !doOneEighty)
        {
            _canOneEighty = true;
        }
        ////////////////////////////////////////////////

        bool driftHeld = TakingInput && myDrift;
        bool driftReleased = _wasDrifting && !driftHeld;

        if (driftHeld)
        {
            if (grounded)
            {
                _canFlip = true;
            }
        }
        else if (driftReleased)
        {
            if (grounded)
                _canFlip = false;
        }
        
        if (_flipTanukiCounter <= 180f) // Tanuki 180 Jump
        {
            _flipTanukiCounter += Time.deltaTime*500f;
            if(!tanukiIsFlipping) tanukiIsFlipping = true;
            tanukiAnimations.transform.localEulerAngles = !tanukiFliped ? new Vector3(0f,(180f + _flipTanukiCounter) * -_tanukiFlipDirection, 0f) : new Vector3(0f, _flipTanukiCounter * -_tanukiFlipDirection, 0f);
        }
        else
        {
            if (tanukiIsFlipping) tanukiIsFlipping = false;
            if (RewindInput.Instance.GetButtonDown(RewindInputActions.JUMP) && !_sliding && !isGrabbing && TakingInput && Time.timeScale > 0.5f) // Tanuki 180 Jump
            {
                if(!tanukiFliped)
                {
                    tanukiAnimations.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                    anim.SetFloat(GrabDir, -1f);
                }
                else
                {
                    anim.SetFloat(GrabDir, 1f);
                }
                _flipTanukiCounter = 0f;
                _tanukiFlipDirection = Mathf.Sign(_lStickInput.x);
                tanukiFliped = !tanukiFliped;
                tanukiAnimations.SetTanukiFlipAnimTrigger();

            }
        }

        if(dirDot > 0.97f && _ridingFakie)
        {
            _ridingFakie = false;
            gainingSpeedEffect.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
        }
        else if(dirDot < -0.97f && !_ridingFakie)
        {
            _ridingFakie = true;
            gainingSpeedEffect.transform.localEulerAngles = new Vector3(-90f, 0f, -180f);
        }

        float currentVelocityMagnitude = rb.linearVelocity.magnitude;
        
        speed0to1 = currentVelocityMagnitude / maxSpeed;
        speed0to1Probable = Mathf.Clamp01((currentVelocityMagnitude - minSpeed) / (maxSpeed - minSpeed));

        dirDot = Vector3.Dot(transform.forward, rb.linearVelocity.normalized);
        dirDotABS = Mathf.Abs(dirDot);
        driftVal = Vector3.Dot(transform.right, rb.linearVelocity.normalized);
        steerDir = lStickInputLerped.x * Mathf.Sign(dirDot);

        _stickDirClampValue = Mathf.Clamp(lStickInputLerped.x * Mathf.Sign(dirDot) - driftVal, -1f, 1f);

        if (speed0to1 > 0.1)
            anim.SetFloat(Steer, _stickDirClampValue);

        if(_stickDirClampValue > 0.9f && grounded)
        {
            if (FR_wheel_PS.isStopped)
                FR_wheel_PS.Play();
            if (BR_wheel_PS.isStopped)
                BR_wheel_PS.Play();

        }
        else
        {
            if (FR_wheel_PS.isPlaying)
                FR_wheel_PS.Stop();
            if (BR_wheel_PS.isPlaying)
                BR_wheel_PS.Stop();
        }

        if (_stickDirClampValue < -0.9f && grounded)
        {
            if (FL_wheel_PS.isStopped)
                FL_wheel_PS.Play();
            if (BL_wheel_PS.isStopped)
                BL_wheel_PS.Play();

        }
        else
        {
            if (FL_wheel_PS.isPlaying)
                FL_wheel_PS.Stop();
            if (BL_wheel_PS.isPlaying)
                BL_wheel_PS.Stop();
        }


        if (Physics.Raycast(fTracksPoint.position, -transform.up, 0.6f) && Physics.Raycast(bTracksPoint.position, -transform.up, 0.6f)) //GROUNDED
        {
            ////////////////////////////// Landing
            if (!grounded)
            {
                var transform1 = transform;
                Instantiate(landingEffect, transform1.position, transform1.rotation);
                _airRotDegCounter = 0f;
                
                rb.angularVelocity = Vector3.zero;
                var velocity1 = rb.linearVelocity;
                velocity1 = new Vector3(velocity1.x, -5f, velocity1.z);
                rb.linearVelocity = velocity1;

                if(_fixLandingCooldown > 0.5f)
                    _fixLandingDir = true;

                _didGrab = false;
                isGrabbing = false;
                _canGrab = true;
                anim.SetBool(Grab, false);
                doFlip = false;
                _onRamp = false;
                _fixLandingCooldown = 0f;
            }

            grounded = true;
            var velocity = rb.linearVelocity;
            _fVelocity = Vector3.Project( new Vector3(velocity.x, velocity.y, velocity.z), transform.forward);
            _tiltVal = bTracksPoint.position.y - fTracksPoint.position.y;
            
            if (dirDotABS < 0.9f)
            {
                if(slideDust_Front_PS.isStopped)
                    slideDust_Front_PS.Play();
                if (slideDust_Back_PS.isStopped)
                    slideDust_Back_PS.Play();
                _sliding = true;
            }
            else
            {
                if (slideDust_Front_PS.isPlaying)
                    slideDust_Front_PS.Stop();
                if (slideDust_Back_PS.isPlaying)
                    slideDust_Back_PS.Stop();
                _sliding = false;
            }

            if (Physics.Raycast(transform.position, -transform.up,out var hit, 1f))
            {
                if (hit.collider.gameObject.CompareTag("Ramp"))
                {
                    _onRamp = true;
                    _rampVector = hit.collider.transform.right;
                }
            }
        }
        else // IN AIR
        {
            _tiltVal = 0f;
            grounded = false;
            slideDust_Front_PS.Stop();
            slideDust_Back_PS.Stop();
            _fVelocity = rb.linearVelocity;
            _lSlideVal = 0.98f;
            rb.angularDamping = 5f;
            if (_sliding) _sliding = false;
            var transform1 = transform;
            transform.rotation = Quaternion.Lerp(transform1.rotation, Quaternion.LookRotation(transform1.forward, Vector3.up), Time.deltaTime * 5f);
            _fixLandingCooldown += Time.deltaTime;
            
            if(RewindInput.Instance.GetButtonDown(RewindInputActions.GRAB))
            {
                isGrabbing = true;
            }
            if(_canGrab && isGrabbing)
            {
                anim.SetBool(Grab, true);
                _didGrab = true;
            }

            if (RewindInput.Instance.GetButtonUp(RewindInputActions.GRAB))
            {
                isGrabbing = false;
                anim.SetBool(Grab, false);
            }

            if(_canFlip)
            {
                if (driftReleased)
                {
                    StartCoroutine(GrabCooldown());
                    doFlip = true;
                    anim.SetTrigger(Flip);
                    flipPS.Play();
                    flipSound.Play();
                    _canFlip = false;
                }
            }
        }

        ////SCORE Stuff/////
        if (Physics.Raycast(transform.position + (transform.up * 0.5f), transform.right,out var scoreHit, 2.1f) && Physics.Raycast(transform.position + (transform.up * 0.5f), -transform.right, out scoreHit, 2.1f))
        {
            if (speed0to1 > 0.6f && !_closeCall && !scoreHit.collider.CompareTag("Road"))
            {
                _closeCall = true;
            }
        }
        else if (Physics.Raycast(transform.position + (transform.up*0.8f) - (transform.forward * (0.4f * Mathf.Sign(dirDot))), transform.right, out scoreHit, 1.4f) || Physics.Raycast(transform.position + (transform.up * 0.8f) - (transform.forward * (0.4f * Mathf.Sign(dirDot))), -transform.right, out scoreHit, 1.4f))
        {
            if(speed0to1 > 0.6f && !_closeCall && !scoreHit.collider.CompareTag("Road"))
            {
                _closeCall = true;
                if (TakingInput)
                {
                    if (scoreHit.collider.tag.ToLower().Contains("car"))
                    {
                        var chanceOfHonk = 0.2f;
                        if (Random.Range(0f, 1f) <= chanceOfHonk) Object.Instantiate(honkSFXPrefab, scoreHit.point, Quaternion.identity);
                    }
                }
            }
        }
        else
        {
            _closeCall = false;
        }

        if (grounded)
        {
            if (!doOneEighty)
            {
                if (dirDotABS < 0.9f && speed0to1 > 0.4f)
                {
                    if (!_gainingPoints)
                    {
                        _gainingPoints = true;
                        SetRumble(driftRumbleValue);
                    }
                }
                else
                {
                    if (_gainingPoints)
                    {
                        _gainingPoints = false;
                        SetRumble(0f);
                        if(_driftSpeedBoostCounter > 0.5f)
                            GentleSpeedup(_driftSpeedBoostCounter);
                        _driftSpeedBoostCounter = 0f;
                    }
                }

                if(_gainingPoints)
                {
                    _driftSpeedBoostCounter += Time.deltaTime*0.5f;
                }
            }
        }
        else
        {
            if (_gainingPoints)
            {
                _gainingPoints = false;
                SetRumble(0f);
                if (_driftSpeedBoostCounter > 0.5f)
                    GentleSpeedup(_driftSpeedBoostCounter);
                _driftSpeedBoostCounter = 0f;
            }
        }

        if (grounded)
        {
            if (speed0to1 > 0.6f && !_overEdge)
            {
                if (!Physics.Raycast(backREdgeDetect.position, Vector3.down, 20f))
                {
                    _overEdge = true;
                    if(!backREdgePS.isPlaying)
                        backREdgePS.Play();
                }
                else
                {
                    if (backREdgePS.isPlaying)
                        backREdgePS.Stop();
                }
                if (!Physics.Raycast(frontREdgeDetect.position, Vector3.down, 20f))
                {
                    _overEdge = true;
                    if (!frontREdgePS.isPlaying)
                        frontREdgePS.Play();
                }
                else
                {
                    if (frontREdgePS.isPlaying)
                        frontREdgePS.Stop();
                }
                if (!Physics.Raycast(backLEdgeDetect.position, Vector3.down, 20f))
                {
                    _overEdge = true;
                    if (!backLEdgePS.isPlaying)
                        backLEdgePS.Play();
                }
                else
                {
                    if (backLEdgePS.isPlaying)
                        backLEdgePS.Stop();
                }
                if (!Physics.Raycast(frontLEdgeDetect.position, Vector3.down, 20f))
                {
                    _overEdge = true;
                    if (!frontLEdgePS.isPlaying)
                        frontLEdgePS.Play();
                }
                else
                {
                    if (frontLEdgePS.isPlaying)
                        frontLEdgePS.Stop();
                }
            }

            if (Physics.Raycast(backREdgeDetect.position, Vector3.down, 10f)
                && Physics.Raycast(frontREdgeDetect.position, Vector3.down, 10f)
                && Physics.Raycast(backLEdgeDetect.position, Vector3.down, 10f)
                && Physics.Raycast(frontLEdgeDetect.position, Vector3.down, 10f))
            {
                if (_overEdge)
                {
                    _overEdge = false;
                }
            }
        }

        _wasDrifting = driftHeld;
    }

    private void FixedUpdate()
    {
        if(_dontTanukiAtAll)
        {
            return;
        }

        _lStickInput = TakingInput ? new Vector2(myHorizontal, myVertical) : Vector2.zero;
        lStickInputLerped = Vector2.Lerp(lStickInputLerped, _lStickInput, Time.fixedDeltaTime * 20f);

        if (_lStickInput.y > 0.8f)
        {
            unlockSpeedLimit = !doOneEighty;
            _slowDown = false;
            _brakeAmount = 0f;
        }
        else if (_lStickInput.y < -0.1f && enableSlowDownFunctionality)
        {
            unlockSpeedLimit = _sliding;
            _slowDown = true;
            _brakeAmount = Mathf.Clamp01((-_lStickInput.y - 0.1f) / 0.9f);
        }
        else
        {
            unlockSpeedLimit = _sliding;
            _slowDown = false;
            _brakeAmount = 0f;
        }

        var currentVelocityMagnitude = rb.linearVelocity.magnitude;
        speed0to1 = currentVelocityMagnitude / maxSpeed;
        speed0to1Probable = Mathf.Clamp01((currentVelocityMagnitude - minSpeed) / (maxSpeed - minSpeed));

        if (currentVelocityMagnitude > 0.001f)
        {
            Vector3 velocityNormalized = rb.linearVelocity / currentVelocityMagnitude;
            dirDot = Vector3.Dot(transform.forward, velocityNormalized);
            dirDotABS = Mathf.Abs(dirDot);
            driftVal = Vector3.Dot(transform.right, velocityNormalized);
        }
        steerDir = lStickInputLerped.x * Mathf.Sign(dirDot);
        _stickDirClampValue = Mathf.Clamp(lStickInputLerped.x * Mathf.Sign(dirDot) - driftVal, -1f, 1f);

        bool driftHeldFixed = TakingInput && myDrift;
        if (driftHeldFixed)
        {
            _lSlideVal = 0.982f;
            _currentTurnPower = turnSlidePower;
            drifting = true;
        }
        else
        {
            if (drifting)
            {
                _currentTurnPower = turnPower;
                drifting = false;
            }
            _lSlideVal = Mathf.Lerp(
                _lSlideVal,
                _slideVal,
                Time.fixedDeltaTime * (tractionCurve.Evaluate(dirDotABS) * 0.5f + tractionCurve.Evaluate(1f - speed0to1) * 0.2f)
            );
        }

        if (grounded)
        {
            _fVelocity = Vector3.Project(rb.linearVelocity, transform.forward);
            _sliding = dirDotABS < 0.9f;
        }

        _distanceTraveled += currentVelocityMagnitude;
        
        Vector3 relativeForward;
        if (Mathf.Sign(dirDot) > 0f)
            relativeForward = transform.forward;
        else
            relativeForward = -transform.forward;
        if (grounded)
        {
            var velocity1 = rb.linearVelocity;
            rb.linearVelocity = Vector3.Lerp(new Vector3(_fVelocity.x,velocity1.y, _fVelocity.z), velocity1, _lSlideVal);

            if (_slowDown)
            {
                if (currentVelocityMagnitude > minSpeed)
                {
                    var velocity = rb.linearVelocity;
                    float brakeFactor = Mathf.Clamp01(Time.fixedDeltaTime * slowDownSpeed * 4f * _brakeAmount);
                    velocity -= velocity * brakeFactor;
                    rb.linearVelocity = velocity;
                }
            }
            else
            {
                if (currentVelocityMagnitude < topHighSpeed)
                {
                    rb.AddForce(relativeForward * (accelMul * ((topHighSpeed - currentVelocityMagnitude) / topHighSpeed)), ForceMode.Acceleration);
                }
            }

            //fix landing
            if(_fixLandingDir)
            {
                _lSlideVal = 1f;
                if (dirDot > 0f)
                {
                    if (dirDot < 0.999f)
                    {
                        if (driftVal > 0f)
                        {
                            transform.Rotate(transform.up, 200f * Time.fixedDeltaTime);
                        }
                        else
                        {
                            transform.Rotate(transform.up, -200f * Time.fixedDeltaTime);
                        }
                    }
                    else
                        _fixLandingDir = false;
                }
                else
                {
                    if (dirDot > -0.999f)
                    {
                        if (driftVal > 0f)
                        {
                            transform.Rotate(transform.up, -200f * Time.fixedDeltaTime);
                        }
                        else
                        {
                            transform.Rotate(transform.up, 200f * Time.fixedDeltaTime);
                        }
                    }
                    else
                        _fixLandingDir = false;
                }
            }

            if (dirDotABS is > 0.2f and < 0.9f && currentVelocityMagnitude < maxSpeed) ////////DRIFT BOOST
            {
                var x = Mathf.Abs(driftVal);
                rb.linearVelocity += ((transform.right * (Mathf.Sign(_stickDirClampValue) * 0.4f)) + (relativeForward)).normalized * (driftBoostAmount * Time.fixedDeltaTime * x);
            }

            if(dirDotABS is > 0.68f and < 0.88f)
            {
                if (!GoodDriftEffect.isPlaying)
                    GoodDriftEffect.Play();
            }
            else
            {
                if (GoodDriftEffect.isPlaying)
                    GoodDriftEffect.Stop();
            }

            if(unlockSpeedLimit && speed0to1Probable > 0.75f)
            {
                if (!gainingSpeedEffect.isPlaying)
                    gainingSpeedEffect.Play();
            }
            else
            {
                if (gainingSpeedEffect.isPlaying)
                    gainingSpeedEffect.Stop();
            }

            if (unlockSpeedLimit || _gentleSpeedupCounter > 0f)
            {
                if (currentVelocityMagnitude > maxSpeed)
                {
                    var velocity = rb.linearVelocity;
                    velocity -= velocity * Time.fixedDeltaTime;
                    rb.linearVelocity = velocity;
                }
                else
                {
                    rb.AddForce(relativeForward * (1.2f * ((maxSpeed - currentVelocityMagnitude)/maxSpeed)), ForceMode.Acceleration);
                }
            }
            else
            {
                if (currentVelocityMagnitude > topHighSpeed)
                {
                    var velocity = rb.linearVelocity;
                    velocity -= velocity * (Time.fixedDeltaTime * 0.05f);
                    rb.linearVelocity = velocity;
                }
            }

            if (doOneEighty)
            {
                if (_oneEightyStartGoofy == _ridingFakie)
                {
                    DoOneEighty();
                }
                else
                {
                    doOneEighty = false;
                }
            }
            else  // TURNING
                transform.Rotate(transform.up, ((_currentTurnPower * driftingCurve.Evaluate(dirDotABS)) * lStickInputLerped.x * Mathf.Clamp01(currentVelocityMagnitude+0.5f) + (crabWiggle * dirDot)) * Time.fixedDeltaTime);
        }
        else
        {
            _airRotDegCounter += rb.angularVelocity.y * Time.fixedDeltaTime * 60f;
            
            rb.AddRelativeTorque(transform.up * (lStickInputLerped.x * turnAirPower), ForceMode.Acceleration);
            if(doOneEighty)
            {
                rb.AddRelativeTorque(transform.up * (turnAirPower * _startOneEightyDir), ForceMode.VelocityChange);
                doOneEighty = false;
            }
            TurnOffEdgeEffect();

            if (GoodDriftEffect.isPlaying)
                GoodDriftEffect.Stop();

            if (_onRamp)
            {
                rb.linearVelocity += _rampVector * (-(Vector3.Dot(rb.linearVelocity.normalized, _rampVector)) * 80f * Time.fixedDeltaTime);
            }
        }
        
        if(speed0to1 < 0.1f && _tiltVal > 0.1f)
        {
            rb.linearVelocity = relativeForward * (3f * Mathf.Sign(dirDot));
        }

        if(_gentleSlowdownCounter > 0f)
        {
            _gentleSlowdownCounter -= Time.fixedDeltaTime;
            var velocity = rb.linearVelocity;
            velocity -= velocity * (Time.fixedDeltaTime * _gentleSlowdownCounter);
            rb.linearVelocity = velocity;
        }
        if (_gentleSpeedupCounter > 0f)
        {
            _gentleSpeedupCounter -= Time.fixedDeltaTime;
            rb.AddForce(relativeForward * (5f * _gentleSpeedupCounter), ForceMode.Acceleration);
            var emission = bitBoostEffect.emission;
            emission.rateOverTime = _gentleSpeedupCounter * 400f;
        }
    }

    private void StartOneEighty()
    {
        _oneEightyStartGoofy = _ridingFakie;

        _startOneEightyDir = _lStickInput.x != 0f ? Mathf.Sign(_lStickInput.x) : 1f;
        doOneEighty = true;
        _canOneEighty = false;
    }

    private void DoOneEighty()
    {
        _lSlideVal = 1f;
        transform.Rotate(Vector3.up, oneEightyRotationSpeed * Time.fixedDeltaTime * Mathf.Clamp01(rb.linearVelocity.magnitude + 0.5f) * _startOneEightyDir);
    }

    public void SetBoardDirection(Vector3 direction)
    {
        doOneEighty = false;
        if(!_ridingFakie)
            transform.forward = direction;
        else
            transform.forward = -direction;
    }

    private IEnumerator GrabCooldown()
    {
        _canGrab = false;
        yield return new WaitForSeconds(0.4f);
        _canGrab = true;
    }

    private void TurnOffEdgeEffect()
    {
        frontLEdgePS.Stop();
        frontREdgePS.Stop();
        backLEdgePS.Stop();
        backREdgePS.Stop();
    }

    public void GentleSlowdown(float amount)
    {
        if (_gentleSlowdownCounter <= (amount - 0.1f))
        {
            _gentleSlowdownCounter = amount;
            if (amount > 0.4f)
            {
                tanukiAnimations.SetBumpAnimTrigger();
            }
        }
    }
    public void GentleSpeedup(float amount)
    {
        _gentleSpeedupCounter += amount;

        if (_gentleSpeedupCounter > 2f)
                _gentleSpeedupCounter = 2f;

    }

    public void DoHighFive()
    {
        highFive = true;
    }

    public void DoRevertBitCombo()
    {
        revertBitCombo = true;
    }

    public void OnInputChange(InputCheck.InputType newInputType)
    {
        drifting = false;
    }
}
