using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class SwitchSaveData
{
    public int Version = 1;
    public Dictionary<string, int> SavedInt = new Dictionary<string, int>();
    public Dictionary<string, float> SavedFloat = new Dictionary<string, float>();
    public Dictionary<string, string> SavedString = new Dictionary<string, string>();
}

public static class SwitchSaveManager
{
    private const string MountName = "TSFS";
    private const string FileName = "TS_SD_V1.sav";
    private static readonly string FilePath = string.Format("{0}:/{1}", MountName, FileName);
#if UNITY_SWITCH
    // Should match the value entered in PlayerSettings > Publishing Settings > Journal Save Data Size
    // Should be smaller than total user save data size
    private const int JournalSaveDataSize = 49152;   // 16 + 32 KB.
    private static nn.account.Uid _userId;
#endif
#if !UNITY_SWITCH
    private static readonly string dataPath = Application.persistentDataPath + "/SavesDir/";
#endif

    private static bool _loadedData = false;
    private static bool _initialized = false;

    public static SwitchSaveData Data = new SwitchSaveData();
    
    private static byte[] SerializeData()
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                binaryWriter.Write(Data.Version);
                binaryWriter.Write(Data.SavedInt.Count);
                binaryWriter.Write(Data.SavedFloat.Count);
                binaryWriter.Write(Data.SavedString.Count);
                foreach (var _keyValuePair in Data.SavedInt)
                {
                    binaryWriter.Write(_keyValuePair.Key);
                    binaryWriter.Write(_keyValuePair.Value);
                }
                foreach (var _keyValuePair in Data.SavedFloat)
                {
                    binaryWriter.Write(_keyValuePair.Key);
                    binaryWriter.Write(_keyValuePair.Value);
                }
                foreach (var _keyValuePair in Data.SavedString)
                {
                    binaryWriter.Write(_keyValuePair.Key);
                    binaryWriter.Write(_keyValuePair.Value);
                }
                return memoryStream.ToArray();
            }
        }
    }

    private static void DeserializeData(byte[] data)
    {
        using (MemoryStream stream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                int version = reader.ReadInt32();
                if (version != 1)
                {
                    // Handle version mismatch if needed
                }

                int intCount = reader.ReadInt32();
                int floatCount = reader.ReadInt32();
                int stringCount = reader.ReadInt32();
                for (int i = 0; i < intCount; i++)
                {
                    Data.SavedInt[reader.ReadString()] = reader.ReadInt32();
                }
                for (int i = 0; i < floatCount; i++)
                {
                    Data.SavedFloat[reader.ReadString()] = reader.ReadSingle();
                }
                for (int i = 0; i < stringCount; i++)
                {
                    Data.SavedString[reader.ReadString()] = reader.ReadString();
                }
            }
        }
    }

    public static void Initialize()
    {
#if UNITY_SWITCH && !UNITY_EDITOR
        if (_initialized)
        {
            return;
        }
        
        // Initialize NN account , user ID and mounting file system
        
        Debug.Log("Initializing NN Account.");
        nn.account.Account.Initialize();
        _initialized = true;
        
        nn.account.UserHandle userHandle = new nn.account.UserHandle();

        if (!nn.account.Account.TryOpenPreselectedUser(ref userHandle))
        {
            nn.Nn.Abort("Failed to open preselected user.");
        }
        
        Debug.Log("Getting user ID.");
        nn.Result result = nn.account.Account.GetUserId(ref _userId, userHandle);
        result.abortUnlessSuccess();
        
        Debug.Log("Mounting save data archive.");
        result = nn.fs.SaveData.Mount(MountName, _userId);
        result.abortUnlessSuccess();
        
        // Initializing save data

        nn.fs.EntryType entryType = 0;
        result = nn.fs.FileSystem.GetEntryType(ref entryType, FilePath);
        if (result.IsSuccess())
        {
            return;
        }

        if (!nn.fs.FileSystem.ResultPathNotFound.Includes(result))
        {
            result.abortUnlessSuccess();
        }

        Debug.LogFormat("Save Manager Initialized: Mounted {0} User {1}", MountName, _userId);
        
#endif
    }

    public static void Unmount()
    {
#if UNITY_SWITCH
        nn.fs.FileSystem.Unmount(MountName);
#endif
    }

    public static void Save()
    {
        byte[] dataByteArray = SerializeData();
        
        #region SwitchSave
#if UNITY_SWITCH && !UNITY_EDITOR
        
                
        // This method prevents the user from quitting the game while saving.
        // This is required for Nintendo Switch Guideline 0080
        // This method must be called on the main thread.
        UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
        
        // The NintendoSDK plug-in uses a FileHandle object for file operations.
        nn.fs.FileHandle handle = new nn.fs.FileHandle();
        
        while (true)
        {
            // Attempt to open the file in write mode.
            nn.Result rresult = nn.fs.File.Open(ref handle, FilePath, nn.fs.OpenFileMode.Write);
            // Check if file was opened successfully.
            if (rresult.IsSuccess())
            {
                Debug.LogFormat("File {0} is open and ready to write", FilePath);
                // Exit the loop because the file was successfully opened.
                break;
            }
            else
            {
                if (nn.fs.FileSystem.ResultPathNotFound.Includes(rresult)) {
                    // Create a file with the size of the encoded data if no entry exists.
                    rresult = nn.fs.File.Create(FilePath, dataByteArray.LongLength);
                    // Check if the file was successfully created.
                    if (!rresult.IsSuccess())
                    {
                        Debug.LogErrorFormat("Failed to create {0}: {1} - Length {2}", FilePath, rresult.ToString(),dataByteArray.LongLength);
                        return;
                    }
                    else
                    {
                        Debug.LogFormat("Created Save File {0}: {1}", FilePath, rresult.ToString());
                    }
                }
                else
                {
                    // Generic fallback error handling for debugging purposes.
                    Debug.LogErrorFormat("Failed to open {0}: {1}", FilePath, rresult.ToString());
                    return;
                }
            }
        }
        
        // Set the file to the size of the binary data.
        nn.Result result = nn.fs.File.SetSize(handle, dataByteArray.LongLength);

        // You do not need to handle this error if you are sure there will be enough space.
        if (nn.fs.FileSystem.ResultUsableSpaceNotEnough.Includes(result))
        {
            Debug.LogErrorFormat("Insufficient space to write {0} bytes to {1}", dataByteArray.LongLength, FilePath);
            return;
        }
        
        // NOTE: Calling File.Write() with WriteOption.Flush incurs two write operations.
        result = nn.fs.File.Write(handle, 0, dataByteArray, dataByteArray.LongLength, nn.fs.WriteOption.Flush);

        // You do not need to handle this error if you are sure there will be enough space.
        // if (nn.fs.ResultUsableSpaceNotEnough.Includes(result))
        // {
        //     Debug.LogErrorFormat("Insufficient space to write {0} bytes to {1}", dataByteArray.LongLength, filePath);
        // }

        // The file must be closed before committing.
        nn.fs.File.Close(handle);

        // Verify that the write operation was successful before committing.
        if (!result.IsSuccess())
        {
            Debug.LogErrorFormat("Failed to write {0}: {1}", FilePath, result.ToString());
            return;
        }
        
        // This method moves the data from the journaling area to the main storage area.
        // If you do not call this method, all changes will be lost when the application closes.
        // Only call this when you are sure that all previous operations succeeded.
        nn.fs.FileSystem.Commit(MountName);

        // End preventing the user from quitting the game while saving. 
        // This is required for Nintendo Switch Guideline 0080
        UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#endif
        #endregion

        #region NotSwitchSave
#if !UNITY_SWITCH
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }

        File.WriteAllBytes(FilePath, dataByteArray);
        Debug.Log("Saved to " + FilePath);
#endif
        #endregion
    }
    
    public static void LoadData()
    {
        if (!_initialized) Initialize();

        Data.SavedInt.Clear();
        Data.SavedFloat.Clear();
        Data.SavedString.Clear();
        
        #region SwitchLoad
#if UNITY_SWITCH && !UNITY_EDITOR
        Debug.Log("Loading data");

        // The NintendoSDK plug-in uses a FileHandle object for file operations.
        nn.fs.FileHandle handle = new nn.fs.FileHandle();
        
        // Attempt to open the file in read-only mode.
        nn.Result result = nn.fs.File.Open(ref handle, FilePath, nn.fs.OpenFileMode.Read);
        if (!result.IsSuccess())
        {
            if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
            {
                Debug.LogFormat("File not found: {0}", FilePath);
                return;
            }
            else
            {
                Debug.LogErrorFormat("Unable to open {0}: {1}", FilePath, result.ToString());
                return;
            }
        }

        // Get the file size.
        long fileSize = 0;
        nn.fs.File.GetSize(ref fileSize, handle);
        // Allocate a buffer that matches the file size.
        byte[] bytedata = new byte[ fileSize ];
        // Read the save data into the buffer.
        nn.fs.File.Read(handle, 0, bytedata, fileSize);
        // Close the file.
        nn.fs.File.Close(handle);

        using (MemoryStream stream = new MemoryStream(bytedata))
        {
            BinaryReader reader = new BinaryReader(stream);
            int intCount = reader.ReadInt32();
            int floatCount = reader.ReadInt32();
            int stringCount = reader.ReadInt32();
            for (int i = 0; i < intCount; i++)
            {
                Data.SavedInt[reader.ReadString()] = reader.ReadInt32();
            }
            for (int i = 0; i < floatCount; i++)
            {
                Data.SavedFloat[reader.ReadString()] = reader.ReadSingle();
            }
            for (int i = 0; i < stringCount; i++)
            {
                Data.SavedString[reader.ReadString()] = reader.ReadString();
            }
        }
        DeserializeData(bytedata);
        Debug.Log("Data Loaded");
#endif
        #endregion
        #region NonSwitchLoad
#if !UNITY_SWITCH
        if (File.Exists(FilePath))
        {
            byte[] bytedata = File.ReadAllBytes(FilePath);
            DeserializeData(bytedata);
        }
#endif
        #endregion

        _loadedData = true;
    }
    
    public static bool HasKey(string key)
    {
        bool haskey = false;
        if (Data.SavedFloat.ContainsKey(key)) haskey = true;
        if (Data.SavedInt.ContainsKey(key)) haskey = true;
        if (Data.SavedString.ContainsKey(key)) haskey = true;
        return haskey;
    }
    
    public static int GetInt(string key, int defaultValue = 0)
    {
        if (!_initialized) Initialize();

        if (!_loadedData) LoadData();

        int returnValue = defaultValue;
        if (!SwitchSaveManager.Data.SavedInt.TryGetValue(key, out returnValue))
        {
            returnValue = PlayerPrefs.GetInt(key, defaultValue);
        }
        return returnValue;
    }

    public static float GetFloat(string key, float defaultValue = 0)
    {
        if (!_initialized) Initialize();

        if (!_loadedData) LoadData();

        float returnValue = defaultValue;
        if (!SwitchSaveManager.Data.SavedFloat.TryGetValue(key, out returnValue))
        {
            returnValue = PlayerPrefs.GetFloat(key, defaultValue);
        }
        return returnValue;
    }

    public static string GetString(string key, string defaultValue = "")
    {
        if (!_initialized) Initialize();

        if (!_loadedData) LoadData();

        string returnValue = defaultValue;
        if (!SwitchSaveManager.Data.SavedString.TryGetValue(key, out returnValue))
        {
            returnValue = PlayerPrefs.GetString(key, defaultValue);
        }
        return returnValue;
    }

    public static void SetInt(string key, int setValue)
    {
        Data.SavedInt[key] = setValue;
    }

    public static void SetFloat(string key, float setValue)
    {
        Data.SavedFloat[key] = setValue;
    }

    public static void SetString(string key, string setValue)
    {
        Data.SavedString[key] = setValue;
    }


    public static void DeleteAll()
    {
        Data.SavedInt.Clear();
        Data.SavedFloat.Clear();
        Data.SavedString.Clear();
        Save();
    }


    public static void DeleteKey(string keyToDel)
    {
        if (Data.SavedInt.ContainsKey(keyToDel)) Data.SavedInt.Remove(keyToDel);
        if (Data.SavedFloat.ContainsKey(keyToDel)) Data.SavedFloat.Remove(keyToDel);
        if (Data.SavedString.ContainsKey(keyToDel)) Data.SavedString.Remove(keyToDel);
    }
}