using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization;

public class LocalizationManager : MonoBehaviour
{
    static LocalizedStringTable localizedDialogueStringTable = new LocalizedStringTable() { TableReference = "dialogue-table" };
    static LocalizedStringTable localizedItemsStringTable = new LocalizedStringTable() { TableReference = "items-table" };
    static LocalizedStringTable localizedTrialsStringTable = new LocalizedStringTable() { TableReference = "trials-table" };
    static LocalizedStringTable localizedUIStringTable = new LocalizedStringTable() { TableReference = "UI-table" };

    static StringTable dialogueTable;
    static StringTable itemsTable;
    static StringTable trialsTable;
    static StringTable UITable;

    public void ChangeLanguage(string languageCode)
    {
        Debug.Log("Language Changed to: "+languageCode);
        
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
        SaveSystem.CurrentSaveData().languageCode = languageCode;
        SaveSystem.SaveGame();
    }

    void OnEnable()
    {
        localizedDialogueStringTable.TableChanged += (StringTable t) => { dialogueTable = t; };
        localizedItemsStringTable.TableChanged += (StringTable t) => { itemsTable = t; };
        localizedTrialsStringTable.TableChanged += (StringTable t) => { trialsTable = t; };
        localizedUIStringTable.TableChanged += (StringTable t) => { UITable = t; };
    }

    public static string GetLocalizedDialogueString(string key)
    {
        if (dialogueTable == null || string.IsNullOrEmpty(key)) return "";

        StringTableEntry entry = dialogueTable.GetEntry(key);

        if (entry == null || string.IsNullOrEmpty(entry.Value) || string.IsNullOrEmpty(entry.LocalizedValue))
        {
            Debug.LogWarning("[LocalizationManager] Empty Value in localization table: dialogue-table key: "+key);
            return "";
        }

        return entry.LocalizedValue;
    }

    public static string GetLocalizedItemsString(string key)
    {
        if (itemsTable == null || string.IsNullOrEmpty(key)) return "";

        StringTableEntry entry = itemsTable.GetEntry(key);

        if (entry == null || string.IsNullOrEmpty(entry.Value) || string.IsNullOrEmpty(entry.LocalizedValue))
        {
            Debug.LogWarning("[LocalizationManager] Empty Value in localization table: items-table key: "+key);
            return "";
        }

        return entry.LocalizedValue;
    }

    public static string GetLocalizedTrialsString(string key)
    {
        if (trialsTable == null || string.IsNullOrEmpty(key)) return "";

        StringTableEntry entry = trialsTable.GetEntry(key);

        if (entry == null || string.IsNullOrEmpty(entry.Value) || string.IsNullOrEmpty(entry.LocalizedValue))
        {
            Debug.LogWarning("[LocalizationManager] Empty Value in localization table: trials-table key: "+key);
            return "";
        }

        return entry.LocalizedValue;
    }

    public static string GetLocalizedUIString(string key)
    {
        if (UITable == null || string.IsNullOrEmpty(key)) return "";

        StringTableEntry entry = UITable.GetEntry(key);

        if (entry == null || string.IsNullOrEmpty(entry.Value) || string.IsNullOrEmpty(entry.LocalizedValue))
        {
            Debug.LogWarning("[LocalizationManager] Empty Value in localization table: UI-table key: "+key);
            return "";
        }

        return entry.LocalizedValue;
    }
}
