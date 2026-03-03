using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UniqueSkillDetails_", menuName = "Scriptable Objects/Player/Active Skill Details")]
public class ActiveUniqueSkillDetailsSO : ScriptableObject
{
    [Header("Main Info")]
    [Space(10)]
    public string activeUniqueSkillName;
    public ActiveSkill activeSkill;
    public Sprite activeUniqueSkillSprite;

    [Space(10)]
    [Header("Sound Details")]
    public SoundEffectSO activeUniqueSkillSoundEffectOne;
    public SoundEffectSO activeUniqueSkillSoundEffectTwo;

    [Space(10)]
    [Header("Levels")]
    [Tooltip("Index 0 = Level 1, Index 1 = Level 2, Index 2 = Level 3")]
    public List<LevelData> levels = new List<LevelData>() { new LevelData(), new LevelData(), new LevelData() };
    [Range(1, 3)] public int currentActiveLevel = 1;

    // --- DESIGN-TIME DEFAULT ---
    [Space(10), Header("Starting Level (Design-Time Default)")]
    [Min(1)] public int startingLevel = 1;

    // --- RUNTIME STATE (NOT SERIALIZED) ---
    [NonSerialized] public int runtimeActiveLevel = 1;

    [Serializable]
    public struct LevelData
    {
        [Space(10)]
        [Header("Skill Detail Text")]
        [TextArea(2, 5)]
        public string activeUniqueSkillDetails;

        [Space(10)]
        [Header("Skill Duration Details")]
        [Min(0f)] public float cooldown;
        [Min(0f)] public float effectiveDuration;

        [Space(10)]
        [Header("Skill Recast Details")]
        [Min(0f)] public float recastCooldown;
        [Min(0f)] public float recastWindowDuration;
        [Min(0)] public int recastRepeatCount;

        [Space(10)]
        [Header("Mana Details")]
        [Min(0)] public int manaCost;
        [Min(0)] public int manaReserveCost;

        [Header("Optional Tunables (extend as needed)")]
        [Tooltip("Generic potency scalar you can interpret per skill (damage, range, etc.).")]
        public float potency; // optional catch-all
        [Tooltip("Optional per-level description / notes for UI.")]
        [TextArea(1, 4)] public string notes;
    }

    public int GetCurrentActiveLevel()
    {
        int max = Mathf.Max(1, levels?.Count ?? 1);
        if (Application.isPlaying) return Mathf.Clamp(runtimeActiveLevel, 1, max);
        return Mathf.Clamp(startingLevel, 1, max); // editor view
    }

    public void SetCurrentActiveLevel(int increment)
    {
        int max = Mathf.Max(1, levels?.Count ?? 1);

        if (Application.isPlaying) runtimeActiveLevel = Mathf.Clamp(GetCurrentActiveLevel() + increment, 1, max);
        else startingLevel = Mathf.Clamp(GetCurrentActiveLevel() + increment, 1, max);
    }

    /// <summary>
    /// Exact setter (use this from gameplay/UI if possible) 
    /// </summary>
    public void SetActiveLevelExact(int level)
    {
        int max = Mathf.Max(1, levels?.Count ?? 1);
        if (Application.isPlaying) runtimeActiveLevel = Mathf.Clamp(level, 1, max);
        else startingLevel = Mathf.Clamp(level, 1, max);
    }

    /// <summary>
    /// Clamp level to valid range and get data. level is 1-based in gameplay.
    /// </summary>
    public LevelData GetLevelData(int level)
    {
        if (levels == null || levels.Count == 0) return default;
        int idx = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        return levels[idx];
    }

    /// <summary>
    /// Build a player-facing description for a given level (1-based).
    /// Hides sections that have zero/empty values.
    /// </summary>
    public string GetDisplayText(int level)
    {
        var d = GetLevelData(level);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{activeUniqueSkillName} - Level {Mathf.Clamp(level, 1, Mathf.Max(1, levels?.Count ?? 1))}");

        if (d.cooldown > 0f || d.effectiveDuration > 0f)
        {
            sb.Append("• ");
            if (d.cooldown > 0f) sb.Append($"Cooldown: {d.cooldown:0.#}s");
            if (d.cooldown > 0f && d.effectiveDuration > 0f) sb.Append("  |  ");
            if (d.effectiveDuration > 0f) sb.Append($"Duration: {d.effectiveDuration:0.#}s");
            sb.AppendLine();
        }

        bool hasRecast = d.recastRepeatCount > 0 && (d.recastCooldown > 0f || d.recastWindowDuration > 0f);
        if (hasRecast)
        {
            sb.AppendLine($"• Recast: {d.recastRepeatCount} times");
            if (d.recastCooldown > 0f) sb.AppendLine($"  – Recast Cooldown: {d.recastCooldown:0.#}s");
            if (d.recastWindowDuration > 0f) sb.AppendLine($"  – Recast Window: {d.recastWindowDuration:0.#}s");
        }

        if (d.manaCost > 0 || d.manaReserveCost > 0)
        {
            sb.Append("• Mana: ");
            if (d.manaCost > 0) sb.Append($"{d.manaCost} cost");
            if (d.manaCost > 0 && d.manaReserveCost > 0) sb.Append("  |  ");
            if (d.manaReserveCost > 0) sb.Append($"{d.manaReserveCost} reserve");
            sb.AppendLine();
        }

        if (Mathf.Abs(d.potency) > Mathf.Epsilon) sb.AppendLine($"• Potency: {d.potency:0.##}");
        if (!string.IsNullOrWhiteSpace(d.notes)) sb.AppendLine($"• {d.notes}");
        return sb.ToString().TrimEnd();
    }

    // ---------- LIFECYCLE ----------
    private void OnEnable()
    {
        // Reset runtime state every domain reload / play start
        runtimeActiveLevel = Mathf.Clamp(startingLevel, 1, Mathf.Max(1, levels?.Count ?? 1));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (levels == null) levels = new List<LevelData>();
        if (levels.Count == 0) levels.Add(new LevelData());
        startingLevel = Mathf.Clamp(startingLevel, 1, Mathf.Max(1, levels.Count));

        // keep runtime in sync while editing (nice for preview)
        if (!Application.isPlaying) runtimeActiveLevel = startingLevel;
    }
#endif
}
