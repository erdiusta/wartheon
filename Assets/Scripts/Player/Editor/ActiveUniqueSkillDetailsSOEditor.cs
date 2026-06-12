#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActiveUniqueSkillDetailsSO))]
public class ActiveUniqueSkillDetailsSOEditorV2 : Editor
{
    SerializedProperty pName;
    SerializedProperty pDetails;
    SerializedProperty pActiveSkill;
    SerializedProperty pPassiveSkill;
    SerializedProperty pSprite;

    SerializedProperty pSfx1;
    SerializedProperty pSfx2;

    SerializedProperty pLevels;
    SerializedProperty pCurrentLevel;

    int selectedLevelIdx;

    void OnEnable()
    {
        pName = serializedObject.FindProperty("activeUniqueSkillName");
        pActiveSkill = serializedObject.FindProperty("activeSkill");
        pPassiveSkill = serializedObject.FindProperty("passiveSkill");
        pSprite = serializedObject.FindProperty("activeUniqueSkillSprite");

        pSfx1 = serializedObject.FindProperty("activeUniqueSkillSoundEffectOne");
        pSfx2 = serializedObject.FindProperty("activeUniqueSkillSoundEffectTwo");

        pLevels = serializedObject.FindProperty("levels");
        pDetails = serializedObject.FindProperty("activeUniqueSkillDetails");
        pCurrentLevel = serializedObject.FindProperty("currentActiveLevel");

        selectedLevelIdx = Mathf.Clamp(selectedLevelIdx, 0, Mathf.Max(0, pLevels.arraySize - 1));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawMainInfo();
        EditorGUILayout.Space(8);
        DrawSoundDetails();
        EditorGUILayout.Space(12);
        DrawLevelsToolbar();
        EditorGUILayout.Space(6);
        DrawCurrentLevelSelector();
        EditorGUILayout.Space(6);
        DrawSelectedLevelBlock();
        EditorGUILayout.Space(8);
        DrawPreview();

        serializedObject.ApplyModifiedProperties();

        // Safety clamp in target
        var so = (ActiveUniqueSkillDetailsSO)target;
        so.currentActiveLevel = Mathf.Clamp(so.currentActiveLevel, 1, Mathf.Max(1, so.levels.Count));
    }

    void DrawMainInfo()
    {
        EditorGUILayout.PropertyField(pName, new GUIContent("Name"));
        EditorGUILayout.PropertyField(pActiveSkill, new GUIContent("Active Skill"));
        EditorGUILayout.PropertyField(pPassiveSkill, new GUIContent("Passive Skill"));
        EditorGUILayout.PropertyField(pSprite, new GUIContent("Sprite"));
    }

    void DrawSoundDetails()
    {
        EditorGUILayout.PropertyField(pSfx1, new GUIContent("SFX 1"));
        EditorGUILayout.PropertyField(pSfx2, new GUIContent("SFX 2"));
    }

    void DrawLevelsToolbar()
    {
        EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            int count = Mathf.Max(1, pLevels.arraySize);
            string[] tabs = new string[count];
            for (int i = 0; i < count; i++) tabs[i] = $"Level {i + 1}";

            int prevIdx = Mathf.Clamp(selectedLevelIdx, 0, count - 1);
            int newIdx = GUILayout.Toolbar(prevIdx, tabs);

            if (newIdx != prevIdx)
            {
                selectedLevelIdx = newIdx;

                // keep the slider in sync (1-based)
                pCurrentLevel.intValue = Mathf.Clamp(newIdx + 1, 1, count);

                // write immediately so UI reflects the change
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+", GUILayout.Width(26))) AddLevel();

            using (new EditorGUI.DisabledScope(pLevels.arraySize == 0))
            {
                if (GUILayout.Button("Dup", GUILayout.Width(40))) DuplicateLevel(selectedLevelIdx);

                if (GUILayout.Button("-", GUILayout.Width(26))) RemoveLevel(selectedLevelIdx);
            }
        }
    }

    void DrawCurrentLevelSelector()
    {
        int max = Mathf.Max(1, pLevels.arraySize);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Current Active Level", GUILayout.Width(160));
            int clamped = Mathf.Clamp(pCurrentLevel.intValue, 1, max);
            int newLevel = EditorGUILayout.IntSlider(clamped, 1, max);

            if (newLevel != pCurrentLevel.intValue)
            {
                pCurrentLevel.intValue = newLevel;
                selectedLevelIdx = Mathf.Clamp(newLevel - 1, 0, pLevels.arraySize - 1);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }
        }
    }

    void DrawSelectedLevelBlock()
    {
        if (pLevels.arraySize == 0) return;

        selectedLevelIdx = Mathf.Clamp(selectedLevelIdx, 0, pLevels.arraySize - 1);
        SerializedProperty level = pLevels.GetArrayElementAtIndex(selectedLevelIdx);

        var pDetails = level.FindPropertyRelative("activeUniqueSkillDetails");

        var pCooldown = level.FindPropertyRelative("cooldown");
        var pEffectiveDuration = level.FindPropertyRelative("effectiveDuration");

        var pRecastCooldown = level.FindPropertyRelative("recastCooldown");
        var pRecastWindowDuration = level.FindPropertyRelative("recastWindowDuration");
        var pRecastRepeatCount = level.FindPropertyRelative("recastRepeatCount");

        var pManaCost = level.FindPropertyRelative("manaCost");
        var pManaReserveCost = level.FindPropertyRelative("manaReserveCost");

        var pPotency = level.FindPropertyRelative("potency");
        var pNotes = level.FindPropertyRelative("notes");

        EditorGUILayout.LabelField($"Level {selectedLevelIdx + 1}", EditorStyles.boldLabel);

        using(new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.PropertyField(pDetails, new GUIContent("Details"));
        }

        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.PropertyField(pCooldown, new GUIContent("Cooldown (s)"));
            EditorGUILayout.PropertyField(pEffectiveDuration, new GUIContent("Effective Duration (s)"));
        }

        using (new EditorGUILayout.VerticalScope("box"))
        {
            // Always editable — no disabled scope
            EditorGUILayout.PropertyField(pRecastCooldown, new GUIContent("Recast Cooldown (s)"));
            EditorGUILayout.PropertyField(pRecastWindowDuration, new GUIContent("Recast Window (s)"));
            EditorGUILayout.PropertyField(pRecastRepeatCount, new GUIContent("Repeat Count"));

            // Small hint instead of disabling fields
            if (pRecastRepeatCount.intValue <= 0)
                EditorGUILayout.HelpBox("Set Repeat Count > 0 to enable recast at runtime.", MessageType.Info);
        }

        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.PropertyField(pManaCost, new GUIContent("Mana Cost"));
            EditorGUILayout.PropertyField(pManaReserveCost, new GUIContent("Reserve Cost"));
        }

        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.PropertyField(pPotency, new GUIContent("Potency"));
            EditorGUILayout.PropertyField(pNotes, new GUIContent("Notes"));
        }
    }

    void DrawPreview()
    {
        var so = (ActiveUniqueSkillDetailsSO)target;
        int levelForPreview = Mathf.Clamp(so.currentActiveLevel, 1, Mathf.Max(1, so.levels.Count));
        string preview = so.GetDisplayText(levelForPreview);

        EditorGUILayout.LabelField("UI Preview", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.HelpBox(preview, MessageType.None);
        }
    }

    void AddLevel()
    {
        pLevels.arraySize++;
        selectedLevelIdx = pLevels.arraySize - 1;
        pCurrentLevel.intValue = selectedLevelIdx + 1;
        serializedObject.ApplyModifiedProperties();
        Repaint();
    }

    void DuplicateLevel(int idx)
    {
        if (pLevels.arraySize == 0) { AddLevel(); return; }
        pLevels.InsertArrayElementAtIndex(idx);
        selectedLevelIdx = Mathf.Clamp(idx + 1, 0, pLevels.arraySize - 1);
        pCurrentLevel.intValue = selectedLevelIdx + 1;
        serializedObject.ApplyModifiedProperties();
        Repaint();
    }

    void RemoveLevel(int idx)
    {
        if (pLevels.arraySize <= 3)
        {
            EditorUtility.DisplayDialog("Cannot Remove", "At least three levels are required.", "OK");
            return;
        }

        pLevels.DeleteArrayElementAtIndex(Mathf.Clamp(idx, 0, pLevels.arraySize - 1));
        selectedLevelIdx = Mathf.Clamp(selectedLevelIdx, 0, pLevels.arraySize - 1);

        // keep slider valid & in-sync
        int count = Mathf.Max(1, pLevels.arraySize);
        pCurrentLevel.intValue = Mathf.Clamp(selectedLevelIdx + 1, 1, count);

        serializedObject.ApplyModifiedProperties();
        Repaint();
    }
}
#endif