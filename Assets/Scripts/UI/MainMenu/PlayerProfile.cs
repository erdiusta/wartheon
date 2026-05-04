using UnityEngine;

public static class PlayerProfile
{
    const string NameKey = "PlayerProfile_Name";
    public static string DisplayName { get; private set; } = string.Empty;

    public static bool IsValid => !string.IsNullOrWhiteSpace(DisplayName);

    public static void Load()
    {
        DisplayName = PlayerPrefs.GetString(NameKey, string.Empty);
    }

    public static void Save(string name)
    {
        DisplayName = name.Trim();
        PlayerPrefs.SetString(NameKey, DisplayName);
        PlayerPrefs.Save();
    }

    public static string GenerateRandomName()
    {
        string[] adjectives = { "Brave", "Silent", "Swift", "Dark", "Fierce", "Lost" };
        string[] nouns = { "Wanderer", "Blade", "Hunter", "Ash", "Rogue", "Traveler" };

        string adj = adjectives[Random.Range(0, adjectives.Length)];
        string noun = nouns[Random.Range(0, nouns.Length)];
        int number = Random.Range(10, 99);

        return $"{adj}{noun}{number}";
    }
}
