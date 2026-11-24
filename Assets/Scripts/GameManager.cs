using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private void Awake ()
    {
        if (_instance != null && _instance != this)
        {
            Destroy (this.gameObject);
        }
        else
        {
            _instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    public LongBoardControls longBoardCTRL;
    public NewGameplayCamera newCamRig;
    public RoadGenerator newRoadGen;
    public ScoreSystem scoreSystem;
    public CollectablesHUD collectablesHUD;
    //public PauseMenu pauseMenu;
    public MissionManagerHUD missionManagerHUD;
    //public ChatBubbleHUD chatBubbleHUD;
    public NewGameplayCamera newGameplayCamera;
    //public BabyRaccoonSystem babyRaccoon;
    //public ProgressMenu progressMenu;
    //public ConversationManager conversationManager;
    public Texture LevelMatCap;
    //public TimeTrialsTimer timeTrialsTimer;
    //public Minimap minimap;
    public PhotoModeCameraMovement PhotoModeCameraMovement;
    public RaccoonAnimations raccoonAnimations;
    public SunAnimatorBPM sunAnimatorBPM;
    public StatusEffectSystem statusEffectSystem;
    public RoadPathCameraAnimation roadPathCameraAnimation;
    //public GhostDataPlayer ghostDataPlayer;
    public string fallbackLevelId;

    public bool isPaused = false;
    public bool isLoadingLevel = false;

    private float timePlayed = 0f;

    private float lastSegmentTime = 0f;
    private float lastSegmentScore = 0f;

    [HideInInspector] public bool skipLevelIntro = false;
   // private StoryLevel currentStoryLevel;

    void Start ()
    {
        GameRoot.Instance.currentGameManager = this;
        Time.timeScale = 1f;
        StartGameplay ();

        //LevelType currentLevelType = LevelManager.Instance.GetCurrentLevelType();

        if (fallbackLevelId.Contains("endless"))
        {
            Debug.LogWarning("Fallback ID contains 'endless' string. Switching mode to ENDLESS. ");
            //LevelManager.Instance.SetEndless();
            //currentLevelType = LevelType.ENDLESS;
        }

        // switch (currentLevelType)
        // {
        //     case (LevelType.STORY):                
        //         InitializeCurrentStoryLevel ();
        //     break;
        //     case (LevelType.TRIAL):
        //         InitializeCurrentTrialLevel ();
        //     break;
        //     case (LevelType.ENDLESS):
        //         InitializeCurrentEndlessLevel ();
        //     break;
        // }

        if (!GameRoot.Instance.hasSoftCheckpointEnabled)
        {
            GameRoot.Instance.currentSegmentTimer = 0f;
            GameRoot.Instance.lastDeathsNumber = 0;
        } else {
            scoreSystem.SetScore(GameRoot.Instance.softCheckpointCarryOverScore);
        }

        // Debug.LogWarning("Current Level Type: "+currentLevelType.ToString());
    }

    void InitializeCurrentStoryLevel ()
    {
        // if (string.IsNullOrEmpty (GameRoot.Instance.mainStoryLibrary.currentStoryLevel.id))
        // {
        //     Debug.LogWarning ("currentStoryLevel on GameRoot is empty, using fallback level ID: " + fallbackLevelId);
        //     GameRoot.Instance.mainStoryLibrary.LoadStoryLevelWithId (fallbackLevelId);
        //     LevelManager.Instance.SetLevel (GameRoot.Instance.mainStoryLibrary.currentStoryLevel.worldId);
        //     LevelManager.Instance.SetCheckpoint (GameRoot.Instance.mainStoryLibrary.currentStoryLevel.checkpointId);
        // }
        //
        // currentStoryLevel = GameRoot.Instance.mainStoryLibrary.currentStoryLevel;
        // pauseMenu.nextCheckpointLevelNumberString = currentStoryLevel.levelNumber;
        //
        // string locLevelName = LocalizationManager.GetLocalizedUIString(currentStoryLevel.levelName);
        // string locLevelDesc = LocalizationManager.GetLocalizedUIString(currentStoryLevel.levelDescription);
        //
        // roadPathCameraAnimation.UpdateIntroCinematicText (locLevelName, locLevelDesc, currentStoryLevel.levelNumber);
    }

    void InitializeCurrentTrialLevel ()
    {
        // Trial currentTrial = LevelManager.Instance.GetCurrentTrial ();
        //
        // if (string.IsNullOrEmpty (currentTrial.trialId))
        // {
        //     Debug.LogWarning ("currentStoryLevel on GameRoot is empty, using fallback level ID: " + fallbackLevelId);
        // }
        //
        // string locLevelName = LocalizationManager.GetLocalizedTrialsString(currentTrial.trialId);
        // string locLevelDesc = LocalizationManager.GetLocalizedUIString("ui-key-good-luck");
        // string locLevelLabel = LocalizationManager.GetLocalizedUIString("ui-key-trials");
        //
        // roadPathCameraAnimation.UpdateIntroCinematicText (locLevelName, locLevelDesc, locLevelLabel);
    }

    void InitializeCurrentEndlessLevel()
    {
        if (GameRoot.Instance.endlessCarryOverDistance > 0f)
        {
            longBoardCTRL.SetCarryOverDistance(GameRoot.Instance.endlessCarryOverDistance);
            GameRoot.Instance.endlessCarryOverDistance = 0f;
        }

        if (GameRoot.Instance.endlessCarryOverScore > 0f)
        {
            scoreSystem.SetScore(GameRoot.Instance.endlessCarryOverScore);
            GameRoot.Instance.endlessCarryOverScore = 0f;
        }

        string locLevelName = LocalizationManager.GetLocalizedUIString("ui-key-endless");
        string locLevelDesc = LocalizationManager.GetLocalizedUIString("ui-key-endless-desc");
        
        roadPathCameraAnimation.UpdateIntroCinematicText (locLevelName, locLevelDesc, locLevelName);
    }

    public int GetCurrentLevelNumber ()
    {
        // if (currentStoryLevel == null) InitializeCurrentStoryLevel ();
        // return currentStoryLevel.worldId + 1;
        return 0;
    }

    private void Update ()
    {
        timePlayed += Time.unscaledDeltaTime;
    }

    private void FixedUpdate ()
    {
        GameRoot.Instance.currentSegmentTimer += Time.fixedDeltaTime;
    }

    public float GetLastSegmentTime ()
    {
        return lastSegmentTime;
    }

    public float GetLastSegmentScore ()
    {
        return lastSegmentScore;
    }

    private void OnDestroy ()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded (Scene scene, LoadSceneMode mode)
    {
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool ("Closed", false);
        
        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.timeScale / 50f;

        // if (GameRoot.Instance.UICanvas.shouldSkipTapeTransition)
        // {
        //     GameRoot.Instance.transitionManager.SkipCasseteTransition ();
        // }
        // else
        // {
        //     GameRoot.Instance.transitionManager.PlayCasseteTransition ();
        // }

        GameRoot.Instance.UICanvas.blackScreenCanvasGroup.FadeTo(this, 0f, 0.5f);
        //pauseMenu.SetGameplayVolume(0f);
    }

    void StartGameplay ()
    {
        //string gameMode = "original";
        //UnityAnalyticsManager.Instance.ReportLevelStart(currentLevel, gameMode);
        timePlayed = 0f;
    }

    void EndGameplay (string reason)
    {
        //if (reason != "retry") LevelManager.Instance.startTriggerCountSinceLastCheckpoint = 0;
        int score = Mathf.RoundToInt (GameManager.Instance.scoreSystem.GetScore ());
        int secondsPlayed = Mathf.RoundToInt (timePlayed);
        //UnityAnalyticsManager.Instance.ReportLevelComplete(currentLevel, score, secondsPlayed, reason);
        timePlayed = 0f;
    }

    public void ResetScene (float delay = 0.25f)
    {
        if (isLoadingLevel) return;

        EndGameplay ("retry");
        StartCoroutine (C_ResetScene (delay));
    }

    public void ResetEndless(float delay = 0.25f)
    {
        if (isLoadingLevel) return;
        
        EndGameplay ("retry");
        StartCoroutine(C_ResetSceneEndless(delay));
    }

    private IEnumerator C_ResetScene (float delay)
    {
        isLoadingLevel = true;
        GameRoot.Instance.UICanvas.shouldSkipTapeTransition = true;
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool ("Closed", true);
        yield return new WaitForSecondsRealtime (delay);
        yield return SceneManager.LoadSceneAsync (SceneManager.GetActiveScene ().buildIndex);
        
        GameRoot.Instance.UICanvas.blackScreenCanvasGroup.alpha = 1f;
        GameRoot.Instance.UICanvas.blackScreenCanvasGroup.FadeTo (this, 0f, 1f, null, false);
        isLoadingLevel = false;
    }

    private IEnumerator C_ResetSceneEndless (float delay)
    {
        isLoadingLevel = true;
        GameRoot.Instance.UICanvas.shouldSkipTapeTransition = true;
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool ("Closed", true);
        yield return new WaitForSecondsRealtime (delay);
        yield return SceneManager.LoadSceneAsync (GameRoot.Instance.endlessSceneNames[0]);
        
        GameRoot.Instance.UICanvas.blackScreenCanvasGroup.alpha = 1f;
        GameRoot.Instance.UICanvas.blackScreenCanvasGroup.FadeTo (this, 0f, 1f, null, false);
        isLoadingLevel = false;
    }

    public void ExitScene ()
    {
        GameRoot.Instance.UICanvas.shouldSkipTapeTransition = false;
        AudioService.Instance.OnGameplaySceneExit ();
        //LevelManager.Instance.startTriggerCountSinceLastCheckpoint = 0;

        GameRoot.Instance.hasSoftCheckpointEnabled = false;
        GameRoot.Instance.currentSegmentTimer = 0f;
        GameRoot.Instance.softCheckpointCarryOverScore = 0f;

        EndGameplay ("exit");
        StartCoroutine (C_LoadScene ("Store"));
    }

    public void LoadScene (int sceneId)
    {
        EndGameplay ("exit");
        StartCoroutine (C_LoadScene (sceneId));
    }

    private void OnApplicationQuit ()
    {
        EndGameplay ("quit");
    }

    void OnNextSceneLoaded (Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool ("Closed", false);
        SceneManager.sceneLoaded -= OnNextSceneLoaded;

        if (scene.name == "Start")
        {
            GameObject.FindObjectOfType<StartScreen> ().GetComponent<Animator> ().SetTrigger ("FastIntro");
        }
    }

    public void HasDied()
    {
        GameRoot.Instance.lastDeathsNumber ++;
    }

    public void UpdateSeagullCompleteStatus()
    {
        //SaveSystem.SubmitSeagullCompletion(currentStoryLevel.worldId);
    }

    public void CheckpointTriggered (bool softCheckpoint = false, string storyLevelClearId = "", bool isEndingCheckpoint = false)
    {
        //string roadLibraryClearId = newRoadGen.levelRoadLibraries[?????]

        if (softCheckpoint)
        {
            //LevelManager.Instance.softCheckpointStartTriggerCount = newRoadGen.startTriggerCountSinceCheckpoint-1;
            GameRoot.Instance.softCheckpointCarryOverScore = scoreSystem.GetScore();
            GameRoot.Instance.hasSoftCheckpointEnabled = true;
            //LevelManager.Instance.startTriggerCountSinceLastCheckpoint = GameManager.Instance.newRoadGen.startTriggerCountSinceCheckpoint;
            return;
        }

        lastSegmentTime = GameRoot.Instance.currentSegmentTimer;
        lastSegmentScore = scoreSystem.GetScore ();
        
        GameRoot.Instance.hasSoftCheckpointEnabled = false;
        GameRoot.Instance.currentSegmentTimer = 0f;
        GameRoot.Instance.softCheckpointCarryOverScore = 0f;

        scoreSystem.ResetCurrentScore ();

        //if (!isEndingCheckpoint) pauseMenu.CheckpointReached ();
        
        //SaveSystem.SubmitLevelRecord (currentStoryLevel.id, lastSegmentTime, (int) lastSegmentScore, LevelRecordType.STORY);
        //SaveSystem.SubmitMedalInfo(currentStoryLevel, lastSegmentTime, Mathf.CeilToInt(lastSegmentScore));

        GameRoot.Instance.lastDeathsNumber = 0;

        //LevelManager.Instance.startTriggerCountSinceLastCheckpoint = 0;
        collectablesHUD.tanukiBitsHUD.SaveTanukiBits ();
        newRoadGen.startTriggerCountSinceCheckpoint = 0;

        //TODO - Decide if story mode or trail mode. make trials unlock items and conversations

        if (storyLevelClearId != null || storyLevelClearId != "")
        {
            SaveData current = SaveSystem.CurrentSaveData ();

            if (!current.unlockedStoryLevelIds.Contains (storyLevelClearId))
            {
                Debug.LogWarning ("Added level to clear list: " + storyLevelClearId);
                current.unlockedStoryLevelIds.Add (storyLevelClearId);
            }

            //GameRoot.Instance.mainStoryLibrary.UpdateCurrentStoryLevel ();
            
            InitializeCurrentStoryLevel();

            SaveSystem.SaveGame ();
        }

        //GameRoot.Instance.achievementManager.CheckForCompletedAchievements();

        if (isEndingCheckpoint)
        {
            SaveProgress();
            return;
        }

        // if (currentStoryLevel?.sceneName != SceneManager.GetActiveScene ().name)
        // {
        //     longBoardCTRL.TakingInput = false;
        //     LoadLevel (GameRoot.Instance.mainStoryLibrary.currentStoryLevel.sceneName, 6f);
        //     LevelManager.Instance.SetCheckpoint (0);
        // }
        // else
        // {
        //     LevelManager.Instance.CheckpointUp ();
        // }

        SaveProgress ();
    }

    public void EndlessCheckpointTriggered ()
    {
        longBoardCTRL.TakingInput = false;

        GameRoot.Instance.endlessCarryOverDistance = longBoardCTRL.GetDistanceTraveled();
        GameRoot.Instance.endlessCarryOverScore = scoreSystem.GetScore();

        //scoreSystem.SetCarryOverScore();
        collectablesHUD.tanukiBitsHUD.SaveTanukiBits ();

        AudioService.Instance.LoadingNextEndlessScene();

        GameRoot.Instance.UICanvas.shouldSkipTapeTransition = true;

        int nextEndlessId = GameRoot.Instance.lastEndlessId;
        
        while (nextEndlessId == GameRoot.Instance.lastEndlessId)
        {
            nextEndlessId = Random.Range(0, GameRoot.Instance.endlessSceneNames.Count);
        }

        GameRoot.Instance.lastEndlessId = nextEndlessId;
        LoadLevel (GameRoot.Instance.endlessSceneNames[nextEndlessId], 2f);
    }

    public void SaveProgress ()
    {
        SaveData current = SaveSystem.CurrentSaveData ();

        //current.currentLevel.levelCheckpoint = LevelManager.Instance.currentCheckpoint;
        //current.currentLevel.levelNumber = LevelManager.Instance.currentLevel;

        SaveSystem.SaveGame ();
        Debug.Log ("Saved checkpoint! LevelID: " + current.currentLevel.levelNumber + "CheckpointID: " + current.currentLevel.levelCheckpoint);
    }

    public void LoadLevel (string name, float delay = 0f)
    {
        EndGameplay ("finished");
        StartCoroutine (C_LoadScene (name, delay));
    }

    IEnumerator C_LoadScene (string name, float delay = 1f)
    {
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool ("Closed", true);
        SceneManager.sceneLoaded += OnNextSceneLoaded;
        yield return new WaitForSecondsRealtime (delay);
        SceneManager.LoadScene (name);
    }

    IEnumerator C_LoadScene (int id, float delay = 1f)
    {
        GameRoot.Instance.UICanvas.circleTransitionAnimator.SetBool ("Closed", true);
        SceneManager.sceneLoaded += OnNextSceneLoaded;
        yield return new WaitForSecondsRealtime (delay);
        SceneManager.LoadScene (id);
    }
}