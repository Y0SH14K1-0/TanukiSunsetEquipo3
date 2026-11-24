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

public class SteamHelper : MonoBehaviour
{
    private static SteamHelper _instance;
    public static SteamHelper Instance { get { return _instance; } }

    private void Awake ()
    {
        if (_instance != null && _instance != this)
        {
            Destroy (this.gameObject);
        }
        else
        {
            _instance = this;
            lastKnownState = new Dictionary<string, ButtonState>();
            thisFrameState = new Dictionary<string, ButtonState>();
        }
    }

    public delegate void OnSteamOverlayHasChanged(bool isActive);
    public OnSteamOverlayHasChanged onSteamOverlayHasChanged;

    #if !DISABLESTEAMWORKS
    protected Callback<GameOverlayActivated_t> m_gameOverlayActivated;
    protected Callback<InputDigitalActionData_t> m_inputDigitalActionData;
    #endif
    private SteamController sc;
    
    private Player rewiredPlayer;
    private const int STEAM_CONTROLLERS_MAX_COUNT = 1;
    private static Dictionary<string, ButtonState> lastKnownState;
    private static Dictionary<string, ButtonState> thisFrameState;

    public enum ButtonState
    {
        UP,
        WAS_UP_NOW_DOWN,
        DOWN,
        WAS_DOWN_NOW_UP
    }

    // States:
    // 0 OFF
    // 1 ONBUTDOWN
    // 2 ONBUT
    // 3 ONBUTUP

    void Start()
    {
        #if !DISABLESTEAMWORKS
        m_gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(IsOverlayActive);
        sc = CurrentSteamController();
        #endif

        rewiredPlayer = ReInput.players.GetPlayer(0);
    }

    public void StoreAchievementStats()
    {
        #if !DISABLESTEAMWORKS

        if (!SteamAPI.Init()) return;
        SteamUserStats.StoreStats();

        #endif
    }

    public void ClearAchievement(string achievementID)
    {
        #if !DISABLESTEAMWORKS

        if (!SteamAPI.Init()) return;
        SteamUserStats.ClearAchievement(achievementID);

        #endif
    }

    public bool GetAchievementStatus(string achievementID)
    {
        #if !DISABLESTEAMWORKS

        if (!SteamAPI.Init()) return false;
        bool result = false;
        SteamUserStats.GetAchievement(achievementID, out result);
        return result;

        #endif
        return false;
    }

    public void SetAchievement(string achievementID)
    {
        #if !DISABLESTEAMWORKS

        if (!SteamAPI.Init()) return;
        SteamUserStats.SetAchievement(achievementID);

        #endif
    }

    private void LateUpdate()
    {
        thisFrameState = new Dictionary<string, ButtonState>();
    }

    #if !DISABLESTEAMWORKS
    void IsOverlayActive(GameOverlayActivated_t pCallback)
    {
        bool isOverlayActive = System.Convert.ToBoolean(pCallback.m_bActive);
        if (onSteamOverlayHasChanged != null) onSteamOverlayHasChanged(isOverlayActive);
    }
    #endif

    private InputCheck.InputType CurrentInputType()
    {
        return GameRoot.Instance.inputCheck.GetInputType();
    }

    private bool IsController()
    {
        return CurrentInputType() == InputCheck.InputType.Controller;
    }

    private bool IsMouseAndKeyboard()
    {
        return CurrentInputType() == InputCheck.InputType.MouseKeyboard;
    }

    public bool GetButtonDown(string steamStringAction, bool forceSteamController = false)
    {
        bool unityBaseValue = rewiredPlayer.GetButtonDown(steamStringAction);

        // Force to ignore Unity input if Steam Input is active and initialized.
        sc = CurrentSteamController();
        if (sc != null && sc.IsInitialized() && IsController()) forceSteamController = true;
        if (forceSteamController) unityBaseValue = false;

        if (thisFrameState.ContainsKey(steamStringAction))
        {
            return (thisFrameState[steamStringAction] == ButtonState.WAS_UP_NOW_DOWN || unityBaseValue);
        }

        if (string.IsNullOrEmpty(steamStringAction) || sc == null) return false || unityBaseValue;

        if (!lastKnownState.ContainsKey(steamStringAction))
        {
            lastKnownState.Add(steamStringAction, 0);
        }

        bool isButtonDown = sc.GetDigitalAction(steamStringAction);
        if (!isButtonDown)
        {
            if (lastKnownState[steamStringAction] == ButtonState.DOWN)
            {
                lastKnownState[steamStringAction] = ButtonState.WAS_DOWN_NOW_UP;
                thisFrameState.Add(steamStringAction, ButtonState.WAS_DOWN_NOW_UP);
                return false || unityBaseValue;
            }
            if (lastKnownState[steamStringAction] == ButtonState.WAS_DOWN_NOW_UP)
            {
                lastKnownState[steamStringAction] = ButtonState.UP;
                thisFrameState.Add(steamStringAction, ButtonState.UP);
                return false || unityBaseValue;
            }
            lastKnownState[steamStringAction] = ButtonState.UP;
            thisFrameState.Add(steamStringAction, ButtonState.UP);
            return false || unityBaseValue;
        }

        ButtonState lastKnownButtonState = lastKnownState[steamStringAction];

        if (lastKnownButtonState == ButtonState.UP || lastKnownButtonState == ButtonState.WAS_DOWN_NOW_UP)
        {
            lastKnownState[steamStringAction] = ButtonState.WAS_UP_NOW_DOWN;
            thisFrameState.Add(steamStringAction, ButtonState.WAS_UP_NOW_DOWN);
            return true;
        }
        
        if (lastKnownButtonState == ButtonState.WAS_UP_NOW_DOWN)
        {
            // Was button down just before, not valid anymore
            lastKnownState[steamStringAction] = ButtonState.DOWN;
            thisFrameState.Add(steamStringAction, ButtonState.DOWN);
            return false || unityBaseValue;
        }

        if (lastKnownButtonState == ButtonState.DOWN)
        {
            // Is holding button
            thisFrameState.Add(steamStringAction, ButtonState.DOWN);
            return false || unityBaseValue;
        }

        return false || unityBaseValue;
    }

    public bool GetButton(string steamStringAction, bool forceSteamController = false)
    {
        bool unityBaseValue = rewiredPlayer.GetButton(steamStringAction);

        // Force to ignore Unity input if Steam Input is active and initialized.
        sc = CurrentSteamController();
        if (sc != null && sc.IsInitialized() && IsController()) forceSteamController = true;
        if (forceSteamController) unityBaseValue = false;

        if (thisFrameState.ContainsKey(steamStringAction))
        {
            return (thisFrameState[steamStringAction] == ButtonState.DOWN) || unityBaseValue;
        }

        if (string.IsNullOrEmpty(steamStringAction) || sc == null) return false || unityBaseValue;

        if (!lastKnownState.ContainsKey(steamStringAction))
        {
            lastKnownState.Add(steamStringAction, 0);
        }

        bool isButtonDown = sc.GetDigitalAction(steamStringAction);
        if (!isButtonDown)
        {
            if (lastKnownState[steamStringAction] == ButtonState.DOWN)
            {
                lastKnownState[steamStringAction] = ButtonState.WAS_DOWN_NOW_UP;
                thisFrameState.Add(steamStringAction, ButtonState.WAS_DOWN_NOW_UP);
                return false || unityBaseValue;
            }
            if (lastKnownState[steamStringAction] == ButtonState.WAS_DOWN_NOW_UP)
            {
                lastKnownState[steamStringAction] = ButtonState.UP;
                thisFrameState.Add(steamStringAction, ButtonState.WAS_DOWN_NOW_UP);
                return false || unityBaseValue;
            }
            lastKnownState[steamStringAction] = ButtonState.UP;
            thisFrameState.Add(steamStringAction, ButtonState.UP);
            return false || unityBaseValue;
        }

        ButtonState lastKnownButtonState = lastKnownState[steamStringAction];

        if (lastKnownButtonState == ButtonState.UP || lastKnownButtonState == ButtonState.WAS_DOWN_NOW_UP)
        {
            lastKnownState[steamStringAction] = ButtonState.WAS_UP_NOW_DOWN;
            thisFrameState.Add(steamStringAction, ButtonState.WAS_UP_NOW_DOWN);
            return true;
        }

        if (lastKnownButtonState == ButtonState.WAS_UP_NOW_DOWN)
        {
            // Was button down just before, not valid anymore
            lastKnownState[steamStringAction] = ButtonState.DOWN;
            thisFrameState.Add(steamStringAction, ButtonState.DOWN);
            return true;
        }

        if (lastKnownButtonState == ButtonState.DOWN)
        {
            // Was button down just before, not valid anymore
            lastKnownState[steamStringAction] = ButtonState.DOWN;
            thisFrameState.Add(steamStringAction, ButtonState.DOWN);
            return true;
        }

        return false || unityBaseValue;
    }

    public bool GetButtonUp(string steamStringAction, bool forceSteamController = false)
    {
        bool unityBaseValue = rewiredPlayer.GetButtonUp(steamStringAction);

        // Force to ignore Unity input if Steam Input is active and initialized.
        sc = CurrentSteamController();
        if (sc != null && sc.IsInitialized() && IsController()) forceSteamController = true;
        if (forceSteamController) unityBaseValue = false;

        if (thisFrameState.ContainsKey(steamStringAction))
        {
            return (thisFrameState[steamStringAction] == ButtonState.WAS_DOWN_NOW_UP) || unityBaseValue;
        }

        if (string.IsNullOrEmpty(steamStringAction) || sc == null) return false || unityBaseValue;

        if (!lastKnownState.ContainsKey(steamStringAction))
        {
            lastKnownState.Add(steamStringAction, 0);
            return false || unityBaseValue;
        }

        bool isButtonDown = sc.GetDigitalAction(steamStringAction);
        if (!isButtonDown)
        {
            if (lastKnownState[steamStringAction] == ButtonState.DOWN)
            {
                lastKnownState[steamStringAction] = ButtonState.WAS_DOWN_NOW_UP;
                thisFrameState.Add(steamStringAction, ButtonState.WAS_DOWN_NOW_UP);
                return true;
            }
            if (lastKnownState[steamStringAction] == ButtonState.WAS_DOWN_NOW_UP)
            {
                lastKnownState[steamStringAction] = ButtonState.UP;
                thisFrameState.Add(steamStringAction, ButtonState.UP);
                return true;
            }
            if (lastKnownState[steamStringAction] == ButtonState.WAS_UP_NOW_DOWN)
            {
                lastKnownState[steamStringAction] = ButtonState.UP;
                thisFrameState.Add(steamStringAction, ButtonState.UP);
                return true;
            }
        }

        if (lastKnownState[steamStringAction] == ButtonState.UP)
        {
            lastKnownState[steamStringAction] = ButtonState.WAS_UP_NOW_DOWN;
            thisFrameState.Add(steamStringAction, ButtonState.WAS_UP_NOW_DOWN);
            return false || unityBaseValue;
        }
        if (lastKnownState[steamStringAction] == ButtonState.WAS_DOWN_NOW_UP)
        {
            lastKnownState[steamStringAction] = ButtonState.WAS_UP_NOW_DOWN;
            thisFrameState.Add(steamStringAction, ButtonState.WAS_UP_NOW_DOWN);
            return false || unityBaseValue;
        }
        if (lastKnownState[steamStringAction] == ButtonState.WAS_UP_NOW_DOWN)
        {
            lastKnownState[steamStringAction] = ButtonState.DOWN;
            thisFrameState.Add(steamStringAction, ButtonState.DOWN);
            return false || unityBaseValue;
        }

        return false || unityBaseValue;
    }

    public float GetAxis(string steamStringAction, bool forceSteamController = false)
    {
        if (string.IsNullOrEmpty(steamStringAction)) return 0f;

        if (
            steamStringAction != RewindInputActions.HORIZONTAL &&
            steamStringAction != RewindInputActions.VERTICAL &&
            steamStringAction != RewindInputActions.R_HORIZONTAL &&
            steamStringAction != RewindInputActions.R_VERTICAL
        )   return 0f;

        float unityAxisResult = rewiredPlayer.GetAxis(steamStringAction);
        float steamAxisResult = 0f;
        sc = CurrentSteamController();

        if (forceSteamController) unityAxisResult = 0f;

        if (sc != null)
        {
            if (steamStringAction == RewindInputActions.HORIZONTAL) steamAxisResult = sc.GetAnalogueAction(SCConsts.LEFT_JOYSTICK_ANALOGUE).x;
            if (steamStringAction == RewindInputActions.VERTICAL) steamAxisResult = sc.GetAnalogueAction(SCConsts.LEFT_JOYSTICK_ANALOGUE).y;
            if (steamStringAction == RewindInputActions.R_HORIZONTAL) steamAxisResult = sc.GetAnalogueAction(SCConsts.RIGHT_JOYSTICK_ANALOGUE).x;
            if (steamStringAction == RewindInputActions.R_VERTICAL) steamAxisResult = sc.GetAnalogueAction(SCConsts.RIGHT_JOYSTICK_ANALOGUE).y;
        }

        return (Mathf.Clamp(unityAxisResult+steamAxisResult, -1f, 1f));
    }

    public float GetAxisRaw(string steamStringAction, bool forceSteamController = false)
    {
        if (string.IsNullOrEmpty(steamStringAction)) return 0f;

        if (
            steamStringAction != RewindInputActions.HORIZONTAL &&
            steamStringAction != RewindInputActions.VERTICAL &&
            steamStringAction != RewindInputActions.R_HORIZONTAL &&
            steamStringAction != RewindInputActions.R_VERTICAL
        )   return 0f;

        float unityAxisResult = rewiredPlayer.GetAxisRaw(steamStringAction);
        float steamAxisResult = 0f;
        sc = CurrentSteamController();

        if (forceSteamController) unityAxisResult = 0f;

        if (sc != null)
        {
            if (steamStringAction == RewindInputActions.HORIZONTAL) steamAxisResult = sc.GetAnalogueAction(SCConsts.LEFT_JOYSTICK_ANALOGUE).x;
            if (steamStringAction == RewindInputActions.VERTICAL) steamAxisResult = sc.GetAnalogueAction(SCConsts.LEFT_JOYSTICK_ANALOGUE).y;
            if (steamStringAction == RewindInputActions.R_HORIZONTAL) steamAxisResult = sc.GetAnalogueAction(SCConsts.RIGHT_JOYSTICK_ANALOGUE).x;
            if (steamStringAction == RewindInputActions.R_VERTICAL) steamAxisResult = sc.GetAnalogueAction(SCConsts.RIGHT_JOYSTICK_ANALOGUE).y;
        }

        return (Mathf.Clamp(unityAxisResult+steamAxisResult, -1f, 1f));
    }

    private SteamController CurrentSteamController()
    {
        #if !DISABLESTEAMWORKS

        if (sc != null) return sc;
        sc = SteamController.Create().Init().First().ActivateActionSet(SCConsts.IN_GAME_ACTION_SET);
        return sc;

        #endif
        return null;
    }

    public SteamController GetCurrentSteamController()
    {
        return CurrentSteamController();
    }
}
