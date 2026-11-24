#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH || UNITY_EDITOR
#define DISABLESTEAMWORKS
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class RewindInput : MonoBehaviour
{
    private static RewindInput _instance;
    public static RewindInput Instance { get { return _instance; } }

    private void Awake ()
    {
        if (_instance != null && _instance != this)
        {
            Destroy (this.gameObject);
        }
        else
        {
            _instance = this;
            Initialize();
        }
    }

    private Player currentPlayer;

    private void Initialize()
    {
        currentPlayer = ReInput.players.GetPlayer(0);
    }
    
    #if DISABLESTEAMWORKS
    // Use Rewired if platform does not need Steam

    public bool GetButton(string buttonId) { return currentPlayer.GetButton(buttonId); }
    public bool GetButtonDown(string buttonId) { return currentPlayer.GetButtonDown(buttonId); }
    public bool GetButtonUp(string buttonId) { return currentPlayer.GetButtonUp(buttonId); }
    public float GetAxis(string axisId) { return currentPlayer.GetAxis(axisId); }
    public float GetAxisRaw(string axisId) { return currentPlayer.GetAxisRaw(axisId); }

    #else
    // Use Steamworks Input Module

    public bool GetButton(string buttonId) { return SteamHelper.Instance.GetButton(buttonId); }
    public bool GetButtonDown(string buttonId) { return SteamHelper.Instance.GetButtonDown(buttonId); }
    public bool GetButtonUp(string buttonId) { return SteamHelper.Instance.GetButtonUp(buttonId); }
    public float GetAxis(string axisId) { return SteamHelper.Instance.GetAxis(axisId); }
    public float GetAxisRaw(string axisId) { return SteamHelper.Instance.GetAxisRaw(axisId); }

    #endif
}

public static class RewindInputActions
{
	public const string HORIZONTAL = "Horizontal";
	public const string VERTICAL = "Vertical";
    public const string R_HORIZONTAL = "RightHorizontal";
    public const string R_VERTICAL = "RightVertical";
    public const string LEFT_BUMPER = "LeftBumper";
    public const string RIGHT_BUMPER = "RightBumper";
    public const string JOYSTICK_PRESS = "JoystickPress";
    public const string PAUSE = "Pause";
	public const string CONFIRM = "Confirm"; 
	public const string BACK = "Back";
    public const string DRIFT = "Drift";
    public const string SWITCH = "Switch";
    public const string GRAB = "Grab";
    public const string JUMP = "Jump";
}
