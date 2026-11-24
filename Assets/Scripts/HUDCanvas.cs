using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

[RequireComponent(typeof(Animator))]

public class HUDCanvas : MonoBehaviour
{
    private Animator hudAnimator;

    public RectTransform hudBarButtonParent;
    public GameObject hudBarButtonPrefab;

    private HUDBarConfiguration currentBarConfiguration;
    private List<HUDBarButton> currentHUDBarButtons;

    private string _selectLocKey = "ui-key-select";
    private string _backLocKey = "ui-key-back";
    private string _startLocKey = "ui-key-start";
    private string _stickerLocKey = "ui-key-stickers-lowercase";
    private string _exitLocKey = "ui-key-exit";
    private string _nextLocKey = "ui-key-next";
    private string _answerLocKey = "ui-key-answer";
    private string _skipLocKey = "ui-key-skip";

    private void Awake()
    {
        hudAnimator = GetComponent<Animator>();  
    }

    private void Start()
    {
        GameRoot.Instance.inputCheck.onInputHasChanged += UpdateBarOnInputChange;
    }

    public void EnableBar()
    {
        //Debug.LogWarning("Enable Bar");
        hudAnimator.SetBool("BarEnabled", true);
    }

    public void DisableBar()
    {
        //Debug.LogWarning("Disable Bar");
        hudAnimator.SetBool("BarEnabled", false);
    }

    public void ConfigureBar(HUDBarConfiguration configuration)
    {
        if (configuration.hudBarButtons.Count <= 0)
        {
            Debug.LogWarning("HUD Configuration is empty. Skipping.");
            return;
        }

        ClearBar();

        foreach (HUDBarConfiguration.HUDBarButtonObject hudButton in configuration.hudBarButtons)
        {
            GameObject newHudButtonGameobject = GameObject.Instantiate(hudBarButtonPrefab, hudBarButtonParent);
            HUDBarButton newHudBarButton = newHudButtonGameobject.GetComponent<HUDBarButton>();
            newHudBarButton.UpdateButtonWithObject(hudButton);
            currentHUDBarButtons.Add(newHudBarButton);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(hudBarButtonParent);
        currentBarConfiguration = configuration;
    }

    public void UpdateBarOnInputChange(InputCheck.InputType newInputType)
    {
        if (currentBarConfiguration == null) return;

        for (int i = 0; i < currentBarConfiguration.hudBarButtons.Count; i++)
        {
            currentHUDBarButtons[i].RefreshButton();
        }
    }

    public void ClearBar()
    {
        currentHUDBarButtons = new List<HUDBarButton>();
        currentBarConfiguration = null;

        for (int i = 0; i < hudBarButtonParent.childCount; i++)
        {
            Destroy(hudBarButtonParent.GetChild(i).gameObject);
        }
    }

    

    public void CreateDefaultBar()
    {
        // Create Open Button
        HUDBarConfiguration.HUDBarButtonObject openButton = new HUDBarConfiguration.HUDBarButtonObject();
        openButton.buttonLabel = LocalizationManager.GetLocalizedUIString(_selectLocKey);
        openButton.spriteId = "Controller_Button_A";

        // Create Back Button
        HUDBarConfiguration.HUDBarButtonObject backButton = new HUDBarConfiguration.HUDBarButtonObject();
        backButton.buttonLabel = LocalizationManager.GetLocalizedUIString(_backLocKey);
        backButton.spriteId = "Controller_Button_B";

        // Create HUD buttons list and add created buttons
        List<HUDBarConfiguration.HUDBarButtonObject> newHudButtons = new List<HUDBarConfiguration.HUDBarButtonObject>();
        newHudButtons.Add(openButton);
        newHudButtons.Add(backButton);

        // Create HUD bar configuration
        HUDBarConfiguration newConfig = new HUDBarConfiguration(newHudButtons);

        // Apply configuration
        ConfigureBar(newConfig);
    }

    public void CreateMapBar()
    {
        // Create Start Button
        HUDBarConfiguration.HUDBarButtonObject startButton = new HUDBarConfiguration.HUDBarButtonObject();
        startButton.buttonLabel = LocalizationManager.GetLocalizedUIString(_startLocKey);
        startButton.spriteId = "Controller_Button_A";

        // Create Back Button
        HUDBarConfiguration.HUDBarButtonObject backButton = new HUDBarConfiguration.HUDBarButtonObject();
        backButton.buttonLabel = LocalizationManager.GetLocalizedUIString(_backLocKey);
        backButton.spriteId = "Controller_Button_B";

        // Create Stickers Button
        HUDBarConfiguration.HUDBarButtonObject stickersInfoButton = new HUDBarConfiguration.HUDBarButtonObject();
        stickersInfoButton.buttonLabel = LocalizationManager.GetLocalizedUIString(_stickerLocKey);
        stickersInfoButton.spriteId = "Controller_Button_X";

        // Create HUD buttons list and add created buttons
        List<HUDBarConfiguration.HUDBarButtonObject> newHudButtons = new List<HUDBarConfiguration.HUDBarButtonObject>();
        newHudButtons.Add(startButton);
        newHudButtons.Add(stickersInfoButton);
        newHudButtons.Add(backButton);

        // Create HUD bar configuration
        HUDBarConfiguration newConfig = new HUDBarConfiguration(newHudButtons);

        // Apply configuration
        ConfigureBar(newConfig);
    }

    public void CreateExitBar()
    {
        // Create Exit Button
        HUDBarConfiguration.HUDBarButtonObject exitButton = new HUDBarConfiguration.HUDBarButtonObject();
        exitButton.buttonLabel = UpperCaseFirstChar(LocalizationManager.GetLocalizedUIString(_exitLocKey));
        exitButton.spriteId = "Controller_Button_A";

        // Create Back Button
        HUDBarConfiguration.HUDBarButtonObject backButton = new HUDBarConfiguration.HUDBarButtonObject();
        backButton.buttonLabel = LocalizationManager.GetLocalizedUIString(_backLocKey);
        backButton.spriteId = "Controller_Button_B";

        // Create HUD buttons list and add created buttons
        List<HUDBarConfiguration.HUDBarButtonObject> newHudButtons = new List<HUDBarConfiguration.HUDBarButtonObject>();
        newHudButtons.Add(exitButton);
        newHudButtons.Add(backButton);

        // Create HUD bar configuration
        HUDBarConfiguration newConfig = new HUDBarConfiguration(newHudButtons);

        // Apply configuration
        ConfigureBar(newConfig);
    }

    public void CreateConversationBar()
    {
        // Create Next Button
        HUDBarConfiguration.HUDBarButtonObject nextButton = new HUDBarConfiguration.HUDBarButtonObject();
        nextButton.buttonLabel = LocalizationManager.GetLocalizedUIString(_nextLocKey);
        nextButton.spriteId = "Controller_Button_A";

        // Create HUD buttons list and add created buttons
        List<HUDBarConfiguration.HUDBarButtonObject> newHudButtons = new List<HUDBarConfiguration.HUDBarButtonObject>();
        newHudButtons.Add(nextButton);

        // Create HUD bar configuration
        HUDBarConfiguration newConfig = new HUDBarConfiguration(newHudButtons);

        // Apply configuration
        ConfigureBar(newConfig);
    }

    public void CreatePhoneBar(bool conversationsAvailable)
    {
        // Create HUD buttons list and add created buttons
        List<HUDBarConfiguration.HUDBarButtonObject> newHudButtons = new List<HUDBarConfiguration.HUDBarButtonObject>();

        if (conversationsAvailable)
        {
            // Create Answer Button
            HUDBarConfiguration.HUDBarButtonObject answerButton = new HUDBarConfiguration.HUDBarButtonObject();
            answerButton.buttonLabel = LocalizationManager.GetLocalizedUIString(_answerLocKey);
            answerButton.spriteId = "Controller_Button_A";
            newHudButtons.Add(answerButton);
        }

        // Create Back Button
        HUDBarConfiguration.HUDBarButtonObject backButton = new HUDBarConfiguration.HUDBarButtonObject();
        backButton.buttonLabel = LocalizationManager.GetLocalizedUIString(_backLocKey);
        backButton.spriteId = "Controller_Button_B";
        newHudButtons.Add(backButton);

        // Create HUD bar configuration
        HUDBarConfiguration newConfig = new HUDBarConfiguration(newHudButtons);

        // Apply configuration
        ConfigureBar(newConfig);
    }

    public void CreateSkipButton()
    {
        // Create Skip Button
        HUDBarConfiguration.HUDBarButtonObject skipButton = new HUDBarConfiguration.HUDBarButtonObject();
        skipButton.buttonLabel = LocalizationManager.GetLocalizedUIString(_skipLocKey);
        skipButton.spriteId = "Controller_Button_Start";


        // Create HUD buttons list and add created buttons
        List<HUDBarConfiguration.HUDBarButtonObject> newHudButtons = new List<HUDBarConfiguration.HUDBarButtonObject>();
        newHudButtons.Add(skipButton);

        // Create HUD bar configuration
        HUDBarConfiguration newConfig = new HUDBarConfiguration(newHudButtons);

        // Apply configuration
        ConfigureBar(newConfig);
    }

    string UpperCaseFirstChar(string text) {
        return Regex.Replace(text, "^[a-z]", m => m.Value.ToUpper());
    }
}

public class HUDBarConfiguration
{
    public List<HUDBarButtonObject> hudBarButtons;

    public HUDBarConfiguration(List<HUDBarButtonObject> _hudBarButtons)
    {
        hudBarButtons = _hudBarButtons;
    }

    public class HUDBarButtonObject
    {
        public string buttonLabel;
        public string spriteId;
    }
}

