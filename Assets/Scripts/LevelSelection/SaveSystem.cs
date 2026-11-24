using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SaveSystem
{
    public static SaveData CurrentSaveData()
    {
        if (currentSaveData == null) LoadGame();
        return currentSaveData;
    }

    private static string oldFileName = "TS_SD_V1.sav";
    private static string saveFileName = "TS_SD.sav";
    private static SaveData currentSaveData;

    private static string tanukiHash = "MonkeysInTheNightYesterdayAndToday";
    private static string lastSaveDataJson;
    private static Coroutine WriteToDiskCoroutine;

    private static MonoBehaviour gameRootMonoBehaviour;

    private static bool useEncryption = true;
    private static bool usePrettyJsonPrint = true;
    private static bool hasCreatedLevelRecords = false;

    public static string SwitchUserId = "";
    public static string SteamUserId = "";

    private const int OLD_VERSION = 1;
    private const int LATEST_VERSION = 2;
    private const bool DEBUG = true;

    // 2021 Update - Divide users to their respective folders
    public static string GetCurrentUserFilePath()
    {
        CheckLegacyFilesToUpdate();
        return Application.persistentDataPath + "/SaveData/" + GetUserFolderName() + "/" + saveFileName;
    }

    public static string GetUserFolderName()
    {
#if UNITY_SWITCH
        return GetSwitchUserId();
#endif
#if UNITY_STANDALONE
        if (SteamManager.Initialized)
        {
            SteamUserId = Steamworks.SteamUser.GetSteamID().ToString();
        } else return "UnknownUsername";
        if (!string.IsNullOrEmpty(SteamUserId)) return SteamUserId;
        else return "UnknownUsername";
#endif
    }

    public static void MatchEncryptionToRuntimeEnvironment()
    {
        useEncryption = TanukiTestRoot.Instance.currentRuntimeEnvironment == TanukiTestRoot.RuntimeEnvironmentType.PRODUCTION;
    }

    public static string GetSwitchUserId()
    {
#if UNITY_SWITCH
        if (SwitchUserId == "")
        {
            nn.account.UserHandle userHandle = new nn.account.UserHandle();
            nn.account.Uid userId = new nn.account.Uid();

            nn.account.Account.GetUserId(ref userId, userHandle);
            SwitchUserId = userId.ToString();
        }
#endif
        return SwitchUserId;
    }

    public static string GetSteamUserId()
    {
#if UNITY_STANDALONE
        if (SteamUserId == "")
        {
            SteamUserId = Steamworks.SteamUser.GetSteamID().ToString();
        }
#endif
        if (!string.IsNullOrEmpty(SteamUserId)) return SteamUserId;
        else return "UnknownUsername";
    }
    public static void ApplicationQuitCleanup()
    {
#if UNITY_SWITCH && !UNITY_EDITOR
        SwitchSaveManager.Unmount();
#endif
    }

    #region Core Save Data Functions

    private static void CheckLegacyFilesToUpdate()
    {
        string tanukiv1filepath = Application.persistentDataPath + "/SaveData/";
        string newPath = Application.persistentDataPath + "/SaveData/" + GetUserFolderName() + "/";

        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
        }

        List<string> oldFilenamePaths = new List<string>();

        oldFilenamePaths.Add(newPath + oldFileName);
        oldFilenamePaths.Add(tanukiv1filepath + oldFileName);

        string newFilenamePath = newPath + saveFileName;
        string backupFilenamePath = newFilenamePath + ".backup";

        foreach (string path in oldFilenamePaths)
        {
            if (!File.Exists(path))
            {
                if (DEBUG) Debug.Log($"No old save data found in old file path.");
                continue;
            }
            else
            {
                if (File.Exists(newFilenamePath))
                {
                    if (File.Exists(backupFilenamePath))
                    {
                        File.Delete(backupFilenamePath);
                    }
                    File.Move(newFilenamePath, backupFilenamePath);
                }

                if (DEBUG) Debug.LogWarning($"Old save data renamed and moved to new path.");

                // Move old Save Data to new location with profile.
                File.Move(path, newFilenamePath);
                break;
            }
        }
    }

    private static SaveData TryLoadData(string saveString)
    {
        string dataToLoad = PrepareDataForLoading(saveString);
        SaveData result = DeserializeSaveData(dataToLoad);
        HandleSaveDataVersion(result);
        return result;
    }

    private static string PrepareDataForLoading(string data)
    {
        if (useEncryption)
        {
            return Cipher.Decrypt(data, tanukiHash);
        }
        return data;
    }

    private static SaveData DeserializeSaveData(string data)
    {
        try
        {
            // First, try to deserialize the data as is.
            SaveData result = JsonUtility.FromJson<SaveData>(data);
            return result;
        }
        catch (System.Exception e)
        {
            if (DEBUG) Debug.LogWarning($"Failed to deserialize save data: {e.Message}. Retrying...");
        }

        // If decryption is enabled, try without decryption (assuming it might already be decrypted).
        if (useEncryption)
        {
            try
            {
                SaveData result = JsonUtility.FromJson<SaveData>(Cipher.Decrypt(data, tanukiHash));
                if (DEBUG) Debug.Log("Successfully loaded data using decryption.");
                return result;
            }
            catch (System.Exception e)
            {
                if (DEBUG) Debug.LogWarning($"Failed to deserialize decrypted save data: {e.Message}. Trying older version...");
            }
        }

        // If the above steps fail, assume the data might be in an older format.
        try
        {
            OldSaveData oldData = JsonUtility.FromJson<OldSaveData>(data);
            // Convert old data format to new one.
            SaveData convertedData = ConvertOldDataToNew(oldData);
            if (DEBUG) Debug.Log("Successfully loaded and converted old version data.");
            currentSaveData = convertedData;
            SaveGame(true);
            return convertedData;
        }
        catch (System.Exception e)
        {
            // If failed, try assuming old data was encrypted.
            try
            {
                OldSaveData decryptedOldData = JsonUtility.FromJson<OldSaveData>(Cipher.Decrypt(data, tanukiHash));
                SaveData convertedData = ConvertOldDataToNew(decryptedOldData);
                if (DEBUG) Debug.Log("Successfully decrypted and loaded old version data.");
                currentSaveData = convertedData;
                SaveGame(true);
                return convertedData;
            }
            catch (System.Exception ex)
            {
                if (DEBUG) Debug.LogError($"Unable to deserialize save data in any format: {ex.Message}");
            }
        }

        // If all attempts fail, return a default instance.
        currentSaveData = new SaveData();
        SaveGame(true);
        return currentSaveData;
    }

    private static SaveData ConvertOldDataToNew(OldSaveData oldData)
    {
        SaveData newData = new SaveData();

        // Direct mapping of properties
        newData.currentLevel = oldData.currentLevel;
        newData.tanukiBitsCollected = oldData.tanukiBitsCollected;
        newData.consumableConversationsIds = oldData.consumableConversationsIds;
        newData.seenConversationLibraryEmittedIds = oldData.seenConversationLibraryEmittedIds;
        newData.unlockedConversationLibrarryEmittedIds = oldData.unlockedConversationLibrarryEmittedIds;
        newData.ownedItemsIds = oldData.ownedItemsIds;
        newData.seenItemsIds = oldData.seenItemsIds;
        newData.unlockedSongsIds = oldData.unlockedSongsIds;
        newData.unlockedStoreItemsIds = oldData.unlockedStoreItemsIds;
        newData.unlockedStoryLevelIds = oldData.unlockedStoryLevelIds;
        newData.unlockedTrialsIds = oldData.unlockedTrialsIds;
        newData.seenTrialsIds = oldData.seenTrialsIds;
        newData.unlockedAchievementIds = oldData.unlockedAchievementIds;
        newData.seenStoreCutscene = oldData.seenStoreCutscene;
        newData.seenBirdIntroCutscene = oldData.seenBirdIntroCutscene;
        newData.seenKitIntroCutscene = oldData.seenKitIntroCutscene;
        newData.equippedDeckId = oldData.equippedDeckId;
        newData.equippedTrucksId = oldData.equippedTrucksId;
        newData.equippedWheelsId = oldData.equippedWheelsId;
        newData.equippedShirtId = oldData.equippedShirtId;
        newData.equippedFacewareId = oldData.equippedFacewareId;
        newData.equippedHatId = oldData.equippedHatId;
        newData.equippedWalkdudeId = oldData.equippedWalkdudeId;
        newData.levelRecords = oldData.levelRecords;
        newData.endlessHighScore = oldData.endlessHighScore;
        newData.endlessTopDistance = oldData.endlessTopDistance;

        // New properties in SaveDatav2
        newData.seenEndCredits = false; // default value
        newData.languageCode = "";      // default value

        return newData;
    }

    private static void HandleSaveDataVersion(SaveData data)
    {
        switch (data.saveDataVersion)
        {
            case OLD_VERSION:
                // Handle old version data
                if (DEBUG) Debug.Log("Loaded old version save data.");
                break;
            case LATEST_VERSION:
                // Handle latest version data
                if (DEBUG) Debug.Log("Loaded latest version save data.");
                break;
            default:
                if (DEBUG) Debug.LogWarning("Unknown save data version.");
                break;
        }
    }

    public static void LoadGame()
    {
        string saveDataString = "";

        MatchEncryptionToRuntimeEnvironment();

#if UNITY_STANDALONE
        string fullPath = GetCurrentUserFilePath();

        if (!File.Exists(fullPath))
        {
            ResetGame();
        }

        saveDataString = File.ReadAllText(fullPath);
#endif

#if UNITY_SWITCH
        string emptySaveData;
        if (useEncryption)
        {
            emptySaveData = Cipher.Encrypt(JsonUtility.ToJson(new SaveData()), tanukiHash);
        } else
        {
            emptySaveData = JsonUtility.ToJson(new SaveData());
        }

        if (DEBUG) Debug.LogWarning("Loading from KEY: "+GetSwitchUserId());
        saveDataString = SwitchSaveManager.GetString("TS_"+GetSwitchUserId(), emptySaveData);
#endif
        currentSaveData = TryLoadData(saveDataString);
    }

    public static void SaveGame(bool forceSave = false)
    {
        if (forceSave)
        {
            string fullPath = GetCurrentUserFilePath();
            string serializedSaveData = JsonUtility.ToJson(currentSaveData, usePrettyJsonPrint);
            string dataToSave;

            MatchEncryptionToRuntimeEnvironment();

            if (useEncryption)
            {
                dataToSave = Cipher.Encrypt(serializedSaveData, tanukiHash);
            } else
            {
                dataToSave = serializedSaveData;
            }
#if UNITY_STANDALONE
            File.WriteAllText(fullPath, dataToSave);        
#endif

#if UNITY_SWITCH
            SwitchSaveManager.SetString("TS_"+GetSwitchUserId(), dataToSave);
            SwitchSaveManager.Save();
#endif
            var dataSize = dataToSave.Length * sizeof(char);
            if (DEBUG) Debug.LogWarning("Wrote save data with size: " + dataSize / 1024 + " KB in: " + fullPath + " [Forced]");
            return;
        }

        if (gameRootMonoBehaviour == null) gameRootMonoBehaviour = GameRoot.Instance;
        if (gameRootMonoBehaviour == null) gameRootMonoBehaviour = GameObject.FindObjectOfType<GameRoot>();

        gameRootMonoBehaviour.StartCoroutine(C_SaveGame());
    }

    private static IEnumerator C_SaveGame()
    {
        if (WriteToDiskCoroutine != null) yield break;

        string serializedSaveData = JsonUtility.ToJson(currentSaveData, usePrettyJsonPrint);
        
        if (serializedSaveData == lastSaveDataJson)
        {
            if (DEBUG) Debug.LogWarning("No changes to save. Skipping.");
            yield break;
        }

        GameRoot.Instance.loadingCanvas.SavingNowAnimator.SetTrigger("Saving");

        string dataToSave;

        MatchEncryptionToRuntimeEnvironment();

        if (useEncryption)
        {
            dataToSave = Cipher.Encrypt(serializedSaveData, tanukiHash);
        }
        else
        {
            dataToSave = serializedSaveData;
        }

        if (gameRootMonoBehaviour == null) gameRootMonoBehaviour = GameRoot.Instance;
        if (gameRootMonoBehaviour == null) gameRootMonoBehaviour = GameObject.FindObjectOfType<GameRoot>();
        WriteToDiskCoroutine = gameRootMonoBehaviour.StartCoroutine(C_WriteToDisk(GetCurrentUserFilePath(), dataToSave));
        yield return WriteToDiskCoroutine;
        lastSaveDataJson = serializedSaveData;
        WriteToDiskCoroutine = null;
    }

    private static IEnumerator C_WriteToDisk(string path, string data)
    {
#if UNITY_STANDALONE
        File.WriteAllText(path, data);
#endif
#if UNITY_SWITCH
        SwitchSaveManager.SetString("TS_"+GetSwitchUserId(), data);
        SwitchSaveManager.Save();
#endif
        var dataSize = data.Length * sizeof(char);
        if (DEBUG) Debug.LogWarning("Wrote save data with size: " + dataSize / 1024 + "KB in: " + GetCurrentUserFilePath());
        yield return null;
    }

    public static void ResetGame()
    {
        currentSaveData = new SaveData();
        SaveGame(true);
    }

    #endregion

    public static void SubmitLevelRecord(string levelId, double totalTime, int totalScore, LevelRecordType recordType)
    {
        LevelRecord currentRecord = FindLevelRecord(levelId);

        if (currentRecord != null)
        {
            // If record already exists
            int existingRecordIndex = currentSaveData.levelRecords.IndexOf(currentRecord);
            if (currentRecord.bestTime == 0) currentRecord.bestTime = totalTime;
            else currentRecord.bestTime = Mathf.Min((float)currentRecord.bestTime, (float)totalTime);
            currentRecord.bestScore = Mathf.Max(currentRecord.bestScore, totalScore);
            currentRecord.isClear = true;

            currentSaveData.levelRecords[existingRecordIndex] = currentRecord;
            SaveGame();
            if (DEBUG) Debug.LogWarning("Overwrote level record for level ID: " + levelId);
            if (DEBUG) Debug.LogWarning("Current level records #: " + currentSaveData.levelRecords.Count);
        }
        else
        {
            // Make new record if not found before
            LevelRecord newLevelRecord = new LevelRecord();
            newLevelRecord.levelId = levelId;
            newLevelRecord.bestTime = totalTime;
            newLevelRecord.bestScore = totalScore;
            newLevelRecord.levelRecordType = recordType;
            newLevelRecord.isClear = true;

            currentSaveData.levelRecords.Add(newLevelRecord);
            SaveGame();
            if (DEBUG) Debug.LogWarning("Saved new level record for level ID: " + levelId);
            if (DEBUG) Debug.LogWarning("Current level records #: " + currentSaveData.levelRecords.Count);
        }
    }

    public static void SubmitEndlessHighscore(float highScore, float distance)
    {
        currentSaveData.endlessHighScore = Mathf.Max(currentSaveData.endlessHighScore, highScore);
        currentSaveData.endlessTopDistance = Mathf.Max(currentSaveData.endlessTopDistance, distance);

        if (currentSaveData.endlessHighScore == highScore)
        {
            // NEW HIGH SCORE
        }

        if (currentSaveData.endlessTopDistance == distance)
        {
            // NEW DISTANCE
        }
        SaveGame();
    }

    public static LevelRecord FindLevelRecord(string levelId)
    {
        return CurrentSaveData().levelRecords.Find(x => x.levelId == levelId);
    }

    private static List<LevelRecord> FindLevelRecordsWithWorldID(int worldId)
    {
        return CurrentSaveData().levelRecords.Where(x => x.worldId == worldId).ToList();
    }

    public static bool HasUnlockedAchievement(string achievementId)
    {
        return currentSaveData.unlockedAchievementIds.Contains(achievementId);
    }

    public static bool HasClearedLevel(string levelId)
    {
        LevelRecord levelRecord = FindLevelRecord(levelId);
        if (levelRecord == null) return false;
        return levelRecord.isClear;
    }

    public static bool HasCompletedLevel(string levelId)
    {
        LevelRecord levelRecord = FindLevelRecord(levelId);
        if (levelRecord == null) return false;
        return levelRecord.isComplete;
    }

    public static void SubmitSeagullCompletion(int currentWorldId)
    {
        List<LevelRecord> worldIdRecords = FindLevelRecordsWithWorldID(currentWorldId);

        foreach(LevelRecord levelRecord in worldIdRecords)
        {
            int existingRecordIndex = currentSaveData.levelRecords.IndexOf(levelRecord);
            levelRecord.seagullMedal = true;
            currentSaveData.levelRecords[existingRecordIndex] = levelRecord;
        }

        SaveGame();
    }
}

[System.Serializable]
public class SaveData
{
    public int saveDataVersion;
    public Level currentLevel;
    public int tanukiBitsCollected;
    public List<string> consumableConversationsIds;
    public List<string> seenConversationLibraryEmittedIds;
    public List<string> unlockedConversationLibrarryEmittedIds;
    public List<string> ownedItemsIds;
    public List<string> seenItemsIds;
    public List<string> unlockedSongsIds;
    public List<string> unlockedStoreItemsIds;
    public List<string> unlockedStoryLevelIds;
    public List<string> unlockedTrialsIds;
    public List<string> seenTrialsIds;
    public List<string> unlockedAchievementIds;
    public bool seenStoreCutscene;
    public bool seenBirdIntroCutscene;
    public bool seenKitIntroCutscene;
    public bool seenEndCredits;
    public string equippedDeckId;
    public string equippedTrucksId;
    public string equippedWheelsId;
    public string equippedShirtId;
    public string equippedFacewareId;
    public string equippedHatId;
    public string equippedWalkdudeId;
    public List<LevelRecord> levelRecords;
    public float endlessHighScore;
    public float endlessTopDistance;
    public string languageCode;

    public SaveData()
    {
        saveDataVersion = 2;
        currentLevel = new Level(0, 0);
        tanukiBitsCollected = 0;
        consumableConversationsIds = new List<string>();
        seenConversationLibraryEmittedIds = new List<string>();
        unlockedConversationLibrarryEmittedIds = new List<string>();
        ownedItemsIds = new List<string>();
        seenItemsIds = new List<string>();
        unlockedSongsIds = new List<string>();
        unlockedStoreItemsIds = new List<string>();
        unlockedStoryLevelIds = new List<string>();
        unlockedTrialsIds = new List<string>();
        seenTrialsIds = new List<string>();
        unlockedAchievementIds = new List<string>();
        levelRecords = new List<LevelRecord>();

        seenStoreCutscene = false;
        seenBirdIntroCutscene = false;
        seenKitIntroCutscene = false;
        seenEndCredits = false;

        equippedDeckId = "deck-default-deck";
        equippedTrucksId = "truck-default-trucks";
        equippedWheelsId = "wheel-green-default";
        equippedShirtId = "shirt-default-shirt";
        equippedFacewareId = "faceware-default-glasses";
        equippedHatId = "hat-default-hat";
        equippedWalkdudeId = "walkdude-default-empty";

        unlockedStoreItemsIds.Add("DEFAULT");

        ownedItemsIds.Add(equippedDeckId);
        ownedItemsIds.Add(equippedTrucksId);
        ownedItemsIds.Add(equippedWheelsId);
        ownedItemsIds.Add(equippedShirtId);
        ownedItemsIds.Add(equippedFacewareId);
        ownedItemsIds.Add(equippedHatId);
        ownedItemsIds.Add(equippedWalkdudeId);

        unlockedStoryLevelIds.Add("mountains-0");

        languageCode = "";
    }
}

[System.Serializable]
public class OldSaveData
{
    public int saveDataVersion;
    public Level currentLevel;
    public int tanukiBitsCollected;
    public List<string> consumableConversationsIds;
    public List<string> seenConversationLibraryEmittedIds;
    public List<string> unlockedConversationLibrarryEmittedIds;
    public List<string> ownedItemsIds;
    public List<string> seenItemsIds;
    public List<string> unlockedSongsIds;
    public List<string> unlockedStoreItemsIds;
    public List<string> unlockedStoryLevelIds;
    public List<string> unlockedTrialsIds;
    public List<string> seenTrialsIds;
    public List<string> unlockedAchievementIds;
    public bool seenStoreCutscene;
    public bool seenBirdIntroCutscene;
    public bool seenKitIntroCutscene;
    public string equippedDeckId;
    public string equippedTrucksId;
    public string equippedWheelsId;
    public string equippedShirtId;
    public string equippedFacewareId;
    public string equippedHatId;
    public string equippedWalkdudeId;
    public List<LevelRecord> levelRecords;
    public float endlessHighScore;
    public float endlessTopDistance;

    public OldSaveData()
    {
        saveDataVersion = 1;
        currentLevel = new Level(0, 0);
        tanukiBitsCollected = 0;
        consumableConversationsIds = new List<string>();
        seenConversationLibraryEmittedIds = new List<string>();
        unlockedConversationLibrarryEmittedIds = new List<string>();
        ownedItemsIds = new List<string>();
        seenItemsIds = new List<string>();
        unlockedSongsIds = new List<string>();
        unlockedStoreItemsIds = new List<string>();
        unlockedStoryLevelIds = new List<string>();
        unlockedTrialsIds = new List<string>();
        seenTrialsIds = new List<string>();
        unlockedAchievementIds = new List<string>();
        levelRecords = new List<LevelRecord>();

        seenStoreCutscene = false;
        seenBirdIntroCutscene = false;
        seenKitIntroCutscene = false;

        equippedDeckId = "deck-default-deck";
        equippedTrucksId = "truck-default-trucks";
        equippedWheelsId = "wheel-green-default";
        equippedShirtId = "shirt-default-shirt";
        equippedFacewareId = "faceware-default-glasses";
        equippedHatId = "hat-default-hat";
        equippedWalkdudeId = "walkdude-default-empty";

        unlockedStoreItemsIds.Add("DEFAULT");

        ownedItemsIds.Add(equippedDeckId);
        ownedItemsIds.Add(equippedTrucksId);
        ownedItemsIds.Add(equippedWheelsId);
        ownedItemsIds.Add(equippedShirtId);
        ownedItemsIds.Add(equippedFacewareId);
        ownedItemsIds.Add(equippedHatId);
        ownedItemsIds.Add(equippedWalkdudeId);

        unlockedStoryLevelIds.Add("mountains-0");
    }
}

[System.Serializable]
public class Level
{
    public int levelNumber;
    public int levelCheckpoint;

    public Level(int levelNumber, int levelCheckpoint)
    {
        this.levelNumber = levelNumber;
        this.levelCheckpoint = levelCheckpoint;
    }
}

[System.Serializable]
public class LevelRecord
{
    public string levelId = "";
    public int worldId = 0;
    public double bestTime = 0f;
    public int bestScore = 0;
    public LevelRecordType levelRecordType;

    public bool seagullMedal = false;
    public bool noDeathMedal = false;
    public bool timeMedal = false;
    public bool scoreMedal = false;
    public bool isClear = false;
    public bool isComplete = false;

    public LevelRecord()
    {
        levelRecordType = LevelRecordType.NULL;
    }
}

[System.Serializable]
public enum LevelRecordType
{
    STORY,
    TIME_TRIAL,
    SCORE_TRIAL,
    NULL
}

