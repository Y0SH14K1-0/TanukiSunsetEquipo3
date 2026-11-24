using UnityEngine;
using System.Collections;
using Steamworks;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

/// <summary>
/// Gareth Williams - Lonewolfwilliams LTD
/// Wraps the Steamcontroller part of the Steamworks API in a simplified, fluent interface
/// Interaction with the Steamcontroller is based on a concept of ActionSets and Actions, where ActionSets define 'contexts' for user interaction and
/// Actions are the actions available to the player in that 'context' ActionSets are defined via a global configuration file (game_actions_<appID>.vdf) and
/// Actions are mapped to controls via the steam client in big-picture mode (big picture mode->library-><game name>->manage game->controller configuration)
/// More info here: https://partner.steamgames.com/documentation/steamcontroller
///
/// You can find the global configuration file for the game in <steam install dir>/controller_config/game_actions_<appID>.vdf
/// More info on install directories here:http://pcgamingwiki.com/wiki/Glossary:Game_data#Steam
/// This wrapper depends on Steamworks.NET
/// https://steamworks.github.io
///
/// NOTE:
/// To test this in-editor, it seems like you need to:
///  - follow the instructions for Steamworks.NET
///  - follow the instructions in the steamworks steamcontroller doc (linked above)
///  - launch the steam client with -forceControllerAppID<My APP> (IE from terminal on mac)
///  - put the steam client in big-picture mode
///  - connect the controller
///  - run the unity game in editor.
/// 
/// I found -dev and -console options are also useful when launching the steam client to verify which controllers and configs were being loaded.
/// 
/// An example of usage could look like this:
/// SteamController sc 	= SteamController.Create().Init().First().ActivateActionSet(SCConsts.IN_GAME_ACTION_SET);
/// bool pausePressed 	= sc.GetDigitalAction(SCConsts.PAUSE_ACTION_DIGITAL);
/// ...rest of code...
/// sc.Shutdown();
/// 
/// </summary>
public class SteamController : MonoBehaviour
{
	private const int UNINITIALISED = -1;
	private int m_currentController = UNINITIALISED;
	private Dictionary<string, ControllerAnalogActionHandle_t> m_cachedActionsAnalogue = new Dictionary<string, ControllerAnalogActionHandle_t>();
	private Dictionary<string, ControllerDigitalActionHandle_t> m_cachedActionsDigital = new Dictionary<string, ControllerDigitalActionHandle_t>();
	private Dictionary<string, ControllerActionSetHandle_t> m_cachedActionSets = new Dictionary<string, ControllerActionSetHandle_t>();
	private Vector2 m_cachedPosition = Vector2.zero;

	private ControllerHandle_t[] m_controllers;
	private bool m_initialised;

	/// <summary>
	/// Create an instance of the SteamController wrapper as a component added to a new GameObject in the root of the scene
	/// </summary>

	public static SteamController Create()
	{
		GameObject go 	= new GameObject();
		go.name 		= "SteamController";
		
		return SteamController.Create (go.transform);
	}

	/// <summary>
	/// Create an instance of the SteamController wrapper as a component added to the specified parent
	/// </summary>
	/// <param name="parent">Parent to add the SteamController component to</param>

	public static SteamController Create(Transform parent)
	{
		return parent.gameObject.AddComponent<SteamController>();
	}

	/// <summary>
	/// Initialise the SteamController API
	/// </summary>

	public SteamController Init()
	{
		if(false == m_initialised) StartCoroutine(InitialiseSteamController());
		return this;
	}

	/// <summary>
	/// Make the first controller the current controller for the SteamController Wrapper
	/// </summary>

	public SteamController First()
	{
		StartCoroutine(WaitForInit(()=>
		{
			m_currentController = 0;
			Debug.Log("got controller: " + m_controllers[m_currentController]);
		}));
		return this;
	}

	/// <summary>
	/// Activates the specified action set.
	/// Action Sets are the actions available to the player in a given 'context'
	/// EG: player may be able to Run and Walk in the 'context' of the Game, but not in the context of the Menu.
	/// Action sets are identified by name in the controller_config .vdf file 
	/// </summary>
	/// <param name="named">The name of the action set as defined in the .vdf config file</param>

	public SteamController ActivateActionSet(string named)
	{
		StartCoroutine(WaitForInit(()=>
		{
			if(false == m_cachedActionSets.ContainsKey(named)) m_cachedActionSets.Add(named, Steamworks.SteamController.GetActionSetHandle(named));
			ControllerActionSetHandle_t h = m_cachedActionSets[named];
			Steamworks.SteamController.ActivateActionSet(m_controllers[m_currentController], h);
			
			//m_currentActionSet = Steamworks.SteamController.GetCurrentActionSet(m_controllers[m_currentController]);
		}));
		return this;
	}

	/// <summary>
	/// Get the state of a named digital action from the currently active action set
	/// Digital actions are actions that can be mapped to buttons (Note: not triggers) on the controller
	/// </summary>
	/// <returns><c>true</c>if the button is down<c>false</c>if the button is up (or the button is not part of the set)</returns>
	/// <param name="named">The name of the action from the actionset as defined in the .vdf config file</param>

	public bool GetDigitalAction(string named)
	{
		if(false == m_initialised) return false;

		if(false == m_cachedActionsDigital.ContainsKey(named)) m_cachedActionsDigital.Add(named, Steamworks.SteamController.GetDigitalActionHandle(named));
		ControllerDigitalActionHandle_t h 	= m_cachedActionsDigital[named]; 
        InputDigitalActionData_t dData = Steamworks.SteamController.GetDigitalActionData(m_controllers[m_currentController], h);

		//if the digital controller isn't part of the ActionSet, it isn't active...
		return System.Convert.ToBoolean(dData.bActive) && System.Convert.ToBoolean(dData.bState);
	}

	/// <summary>
	/// Get the state of a named analogue action from the currently active action set
	/// Analogue actions are actions that can be mapped to either the thumbstick, trackpads, triggers or gyro on the controller
	/// </summary>
	/// <returns>the x and y values for the analogue action (will be zero if the action is not part of the set)</returns>
	/// <param name="named">The name of the action from the actionset as defined in the .vdf config file</param>

	public Vector2 GetAnalogueAction(string named)
	{
		if(false == m_initialised) return Vector2.zero;

		if(false == m_cachedActionsAnalogue.ContainsKey(named)) m_cachedActionsAnalogue.Add(named, Steamworks.SteamController.GetAnalogActionHandle(named));
		ControllerAnalogActionHandle_t h 	= m_cachedActionsAnalogue[named];
		InputAnalogActionData_t aData 	= Steamworks.SteamController.GetAnalogActionData(m_controllers[m_currentController], h);

		//if the analogue controller isn't part of the ActionSet, x and y will be 0;
		m_cachedPosition.x = aData.x;
		m_cachedPosition.y = aData.y;
		return m_cachedPosition;
	}

	/// <summary>
	/// Shutdown the steamcontroller API and clear internal state of wrapper
	/// (NOTE: this does not shut down the Steam API)
	/// </summary>

	public SteamController Shutdown()
	{
		Debug.Log("shutting down");
		Steamworks.SteamController.Shutdown();
		m_cachedActionsAnalogue.Clear ();
		m_cachedActionsDigital.Clear ();
		m_cachedActionSets.Clear ();
		m_currentController = UNINITIALISED;
		m_cachedPosition 	= Vector2.zero;
		m_controllers 		= new ControllerHandle_t[16];
		return this;
	}

	private IEnumerator InitialiseSteamController()
	{
		while(false == SteamManager.Initialized)
		{
			//Debug.Log("waiting for api...");
		 	yield return null;
		}
		Debug.Log("initialised with user: " + SteamFriends.GetPersonaName() + " app id: " + Steamworks.SteamUtils.GetAppID());
		Debug.Log("is steam running: " + Steamworks.SteamAPI.IsSteamRunning());
		Steamworks.SteamController.Init();

		m_controllers = new ControllerHandle_t[Constants.STEAM_CONTROLLER_MAX_COUNT];

		int found = 0;
		while(found == 0)
		{
			Debug.Log("waiting for at least one active controller...");
			found = Steamworks.SteamController.GetConnectedControllers(m_controllers);
			yield return null;
		}
		
		Debug.Log("controller found - Initialised");
		m_initialised = true;
	}

	private IEnumerator WaitForInit(Action then)
	{
		while(false == m_initialised) yield return null;
		then();
	}

	public bool IsInitialized()
	{
		return m_initialised;
	}
}

/// <summary>
/// Helper class defines constants specific to the game configuration (.vdf file)
/// </summary>

public class SCConsts
{
	public const string IN_GAME_ACTION_SET = "GameControls";
	public const string LEFT_JOYSTICK_ANALOGUE = "LeftJoystick";
	public const string RIGHT_JOYSTICK_ANALOGUE = "RightJoystick"; 
	public const string XBOX_A_BUTTON_DIGITAL_ACTION = "XboxAButton";
    public const string XBOX_B_BUTTON_DIGITAL_ACTION = "XboxBButton";
    public const string XBOX_X_BUTTON_DIGITAL_ACTION = "XboxXButton";
    public const string XBOX_Y_BUTTON_DIGITAL_ACTION = "XboxYButton";
    public const string XBOX_PAUSE_BUTTON_DIGITAL_ACTION = "XboxPauseButton";
    public const string XBOX_LEFT_BUMPER_DIGITAL_ACTION = "XboxLeftBumper";
    public const string XBOX_RIGHT_BUMPER_DIGITAL_ACTION = "XboxRightBumper";
}