using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    private static GameRoot _instance;
    public static GameRoot Instance { get { return _instance; } }

    //public Conversation conversationSystem;
    public LoadingCanvas loadingCanvas;
    public UICanvas UICanvas;
    //public TransitionManager transitionManager;
    //public StoreNavigation storeNavigation {
    //    get { return (_storeNavigation != null) ? _storeNavigation : _storeNavigation = GameObject.FindObjectOfType<StoreNavigation>(); }
    //}
    //public GameSession currentGameSession;
    [HideInInspector] public GameManager currentGameManager;

    public AudioSource altSFXaudioSource;
    public AudioSource endingBGMaudioSource;
    //public StoryLibrary mainStoryLibrary;
    public List<string> endlessSceneNames;
    public int lastEndlessId = 0;
    //public StoreItemLibrary boardLibrary;
    //public StoreItemLibrary wardrobeLibrary;
    //public AchievementManager achievementManager;
    public SteamHelper steamHelper;
    public LocalizationManager localizationManager;

    //public ConversationBackground conversationBackground;
    public HUDCanvas hudCanvas;
    public InputCheck inputCheck;
    public HUDSpriteLibrary hudSpriteLibrary;

    public float currentSegmentTimer = 0f;
    public bool hasSoftCheckpointEnabled = false;
    public int lastDeathsNumber = 0;
    public float softCheckpointCarryOverScore = 0f;
    public float endlessCarryOverScore = 0f;
    public float endlessCarryOverDistance = 0f;

    public RuntimeEnvironmentType currentRuntimeEnvironment;
    public GameObject ingameDebugConsoleObject;
    public MagazineCapture magazineCapture;

    //private StoreNavigation _storeNavigation;

    public delegate void OnHasUnlockedNewItemDelegate(List<string> unlockedItemStoreIds);
    public static OnHasUnlockedNewItemDelegate OnHasUnlockedNewItem;

    public delegate void OnHasUnlockedNewConversationDelegate(string unlockedConversationId);
    public static OnHasUnlockedNewConversationDelegate OnHasUnlockedNewConversation;

    //public delegate void OnNewConversationMessageDelegate(ConversationMessage conversationMessage);
    //public OnNewConversationMessageDelegate OnNewConversationMessage;

    private void OnEnable()
    {
        SaveSystem.MatchEncryptionToRuntimeEnvironment();
    }

    private void OnApplicationQuit()
    {
        SaveSystem.ApplicationQuitCleanup();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Cursor.visible = false;
        //SaveSystem.CreateLevelRecordsFromStoryLibrary();

        if (currentRuntimeEnvironment == RuntimeEnvironmentType.PRODUCTION)
        {
            ingameDebugConsoleObject.SetActive(false);
        } else {
            ingameDebugConsoleObject.SetActive(true);
        }
    }

    public string GetGameVersion()
    {
        string runtimeModeSuffix = (currentRuntimeEnvironment == RuntimeEnvironmentType.PRODUCTION) ? "-PROD" : "-DEV";
        return "v"+Application.version+runtimeModeSuffix;
    }

    public void UnlockConversation(string conversationID)
    {
        SaveData currentSaveData = SaveSystem.CurrentSaveData();
        if (!currentSaveData.unlockedConversationLibrarryEmittedIds.Contains(conversationID))
        {
            currentSaveData.unlockedConversationLibrarryEmittedIds.Add(conversationID);
            SaveSystem.SaveGame();
            if (OnHasUnlockedNewConversation != null) OnHasUnlockedNewConversation(conversationID);
        }
    }

    public static void ForceConversation(string conversationID)
    {
        SaveData currentSaveData = SaveSystem.CurrentSaveData();
        if (!currentSaveData.unlockedConversationLibrarryEmittedIds.Contains(conversationID))
        {
            currentSaveData.unlockedConversationLibrarryEmittedIds.Add(conversationID);
        }
        if (currentSaveData.seenConversationLibraryEmittedIds.Contains(conversationID))
        {
            currentSaveData.seenConversationLibraryEmittedIds.Remove(conversationID);
        }
        SaveSystem.SaveGame();
    }

    public void UnlockTrial (string trialId)
    {
        SaveData currentSaveData = SaveSystem.CurrentSaveData();
        if (!currentSaveData.unlockedTrialsIds.Contains(trialId))
            currentSaveData.unlockedTrialsIds.Add(trialId);
        Debug.LogWarning("Unlocking trial with ID: '" + trialId + "'");
        SaveSystem.SaveGame();
    }

    public void UnlockStoreItem(string storeLockId)
    {
        SaveData currentSaveData = SaveSystem.CurrentSaveData();

        if (currentSaveData.unlockedStoreItemsIds.Contains(storeLockId)) return;
        
        List<string> unlockedItemIds = new List<string>();

        bool skipSave = false;

        /*foreach(StoreItem item in boardLibrary.storeItems)
        {
            if (item.lockID == storeLockId)
            {
                if (currentSaveData.unlockedStoreItemsIds.Contains(item.lockID))
                {
                    skipSave = true;
                    continue;
                }
                else skipSave = false;
                currentSaveData.unlockedStoreItemsIds.Add(item.lockID);
                unlockedItemIds.Add(item.lockID);
            }
        }

        foreach (StoreItem item in wardrobeLibrary.storeItems)
        {
            if (item.lockID == storeLockId)
            {
                if (currentSaveData.unlockedStoreItemsIds.Contains(item.lockID))
                {
                    skipSave = true;
                    continue;
                }
                else skipSave = false;
                currentSaveData.unlockedStoreItemsIds.Add(item.lockID);
                unlockedItemIds.Add(item.lockID);
            }
        }*/

        if (skipSave)
        {
            Debug.LogWarning("Unlocked items with lock ID: " + storeLockId + " already unlocked. Skipping Save.");
        } else
            SaveSystem.SaveGame();
        if (OnHasUnlockedNewItem != null) OnHasUnlockedNewItem(unlockedItemIds);
    }

    public static string FloatToTimeFormat(float timeCount)
    {
        int minutes = Mathf.FloorToInt(timeCount / 60f);
        int seconds = Mathf.FloorToInt(timeCount % 60f);
        float fraction = ((timeCount - seconds) * 1000f) % 999f;
        return string.Format("{0:00}:{1:00}:{2}", minutes, seconds, fraction.ToString("000"));
    }

    public enum RuntimeEnvironmentType
    {
        DEVELOPMENT,
        PRODUCTION
    }
}