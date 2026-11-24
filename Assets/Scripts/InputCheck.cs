using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputCheck : MonoBehaviour
{
    public delegate void OnInputHasChanged(InputType newInputType);
    public OnInputHasChanged onInputHasChanged;

    public delegate void OnControllerHasConnected(string newControllerName);
    public OnControllerHasConnected onControllerHasConnected;

    public delegate void OnControllerHasDisconnected(int controllerNumber);
    public OnControllerHasDisconnected onControllerHasDisconnected;

    private float inputCheckCooldown = 0.5f;
    private float currentCooldown = 0.2f;
    private bool _controllerIsConnected = false;

    public enum InputType
    {
        MouseKeyboard,
        Controller
    };

    private InputType InputState = InputType.Controller;

    void FixedUpdate()
    {
        currentCooldown -= Time.fixedDeltaTime;

        if (currentCooldown > 0f) return;

        switch (InputState)
        {
            case InputType.MouseKeyboard:
            if (IsControllerInput())
            {
                currentCooldown = inputCheckCooldown;
                InputState = InputType.Controller;
                if (onInputHasChanged != null) onInputHasChanged(InputState);
            }
            break;
            case InputType.Controller:
            if (IsMouseKeyboard())
            {
                currentCooldown = inputCheckCooldown;
                InputState = InputType.MouseKeyboard;
                if (onInputHasChanged != null) onInputHasChanged(InputState);
            }
            break;
        }
    }

    public InputType GetInputType()
    {
        return InputState;
    }

    private bool IsMouseKeyboard()
    {
        if (Input.GetKey(KeyCode.Escape) ||
            Input.GetKey(KeyCode.Alpha1) ||
            Input.GetKey(KeyCode.Alpha2) ||
            Input.GetKey(KeyCode.Alpha3) ||
            Input.GetKey(KeyCode.Alpha4) ||
            Input.GetKey(KeyCode.Alpha5) ||
            Input.GetKey(KeyCode.Alpha6) ||
            Input.GetKey(KeyCode.Alpha7) ||
            Input.GetKey(KeyCode.Alpha8) ||
            Input.GetKey(KeyCode.Alpha9) ||
            Input.GetKey(KeyCode.Alpha0) ||
            Input.GetKey(KeyCode.Minus) ||
            Input.GetKey(KeyCode.Plus) ||
            Input.GetKey(KeyCode.Tab) ||
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift) ||
            Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.RightControl) ||
            Input.GetKey(KeyCode.PageUp) ||
            Input.GetKey(KeyCode.PageDown) ||
            Input.GetKey(KeyCode.Home) ||
            Input.GetKey(KeyCode.End) ||
            Input.GetKey(KeyCode.Delete) ||
            Input.GetKey(KeyCode.Backspace) ||
            Input.GetKey(KeyCode.KeypadEnter) ||
            Input.GetKey(KeyCode.Space) ||
            Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.UpArrow) ||
            Input.GetKey(KeyCode.DownArrow) ||
            Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D) ||
            Input.GetMouseButton(0) ||
            Input.GetMouseButton(1) ||
            Input.GetMouseButton(2))
        {
            return true;
        }
        return false;
    }

    private bool IsControllerInput()
    {
        if (
            Input.GetKey(KeyCode.Joystick1Button1) ||
            Input.GetKey(KeyCode.Joystick1Button2) ||
            Input.GetKey(KeyCode.Joystick1Button3) ||
            Input.GetKey(KeyCode.Joystick1Button4) ||
            Input.GetKey(KeyCode.Joystick1Button5) ||
            Input.GetKey(KeyCode.Joystick1Button6) ||
            Input.GetKey(KeyCode.Joystick1Button7) ||
            Input.GetKey(KeyCode.Joystick1Button8) ||
            Input.GetKey(KeyCode.Joystick1Button9) ||
            Input.GetKey(KeyCode.Joystick1Button10) ||
            Input.GetKey(KeyCode.Joystick1Button11) ||
            Input.GetKey(KeyCode.Joystick1Button12) ||
            Input.GetKey(KeyCode.Joystick1Button13) ||
            Input.GetKey(KeyCode.Joystick1Button14) ||
            Input.GetKey(KeyCode.Joystick1Button15) ||
            Input.GetKey(KeyCode.Joystick1Button16) ||
            Input.GetKey(KeyCode.Joystick1Button17) ||
            Input.GetKey(KeyCode.Joystick1Button18) ||
            Input.GetKey(KeyCode.Joystick1Button19)
            )
        {
            return true;
        }
        return false;
    }

    void Start()
    {
        StartCoroutine(C_JoystickCheckLoop());
    }

    IEnumerator C_JoystickCheckLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            string[] temp = Input.GetJoystickNames();
            
            if(temp.Length > 0)
            {
                for(int i =0; i < temp.Length; ++i)
                {
                    if(!string.IsNullOrEmpty(temp[i]))
                    {
                        if (!_controllerIsConnected)
                        {
                            //Not empty, controller temp[i] is connected
                            Debug.LogWarning("[InputCheck.cs] Controller " + i + " is Connected using: " + temp[i]);
                            _controllerIsConnected = true;
                            if (onControllerHasConnected != null) onControllerHasConnected(temp[i]);
                        }
                    }
                    else
                    {
                        if (_controllerIsConnected && i == 0)
                        {
                            //If it is empty, controller i is disconnected
                            //where i indicates the controller number
                            Debug.LogWarning("[InputCheck.cs] Controller: " + i + " is Disconnected.");
                            _controllerIsConnected = false;
                            if (onControllerHasDisconnected != null) onControllerHasDisconnected(i);
                        }
                    }
                }
            }
        }
    }
}