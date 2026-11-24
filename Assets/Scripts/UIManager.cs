using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

    private void Awake ()
    {
        if (_instance != null && _instance != this)
        {
            Destroy (this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public CanvasGroup generalCanvasGroup;

    public CanvasGroup speedometerCanvasGroup;
    public CanvasGroup minimapCanvasGroup;

    private void Start()
    {
        float targetAlpha =  PlayerPrefs.GetString("HideHUD", "false") == "true" ? 0f : 1f;
        speedometerCanvasGroup.alpha = targetAlpha;
        minimapCanvasGroup.alpha = targetAlpha;
    }
}