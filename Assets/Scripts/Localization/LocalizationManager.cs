using System;
using UnityEngine.Localization.Settings;

public static class LocalizationManager
{
    public static Language CurrentLanguage;
    public static Action<Language> LanguageChanged;

    public static string GetText(string tableName, string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
    }

    public static void RefreshAllTexts()
    {

    }
}
