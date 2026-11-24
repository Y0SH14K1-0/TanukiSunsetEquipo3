using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TanukiTestRoot : MonoBehaviour
{
    private static TanukiTestRoot _instance;
    public static TanukiTestRoot Instance { get { return _instance; } }
    
    private void Awake()
    {
        if (_instance != null && _instance != this) Destroy(this.gameObject);
        else _instance = this;
    }

    private void Start()
    {
        longBoardControls.EnableGameplay();
    }

    public LongBoardControls longBoardControls;
    public RoadGenerator RoadGenerator;
    public InputCheck InputCheck;
    public RaccoonAnimations RaccoonAnimations;
    public SunAnimatorBPM sunAnimatorBPM;
    public RuntimeEnvironmentType currentRuntimeEnvironment = RuntimeEnvironmentType.DEVELOPMENT;
    
    public enum RuntimeEnvironmentType
    {
        DEVELOPMENT,
        PRODUCTION
    }
}
