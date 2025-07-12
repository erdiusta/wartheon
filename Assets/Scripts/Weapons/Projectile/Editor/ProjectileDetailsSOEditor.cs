using UnityEditor;

[CustomEditor(typeof(ProjectileDetailsSO))]
public class ProjectileDetailsSOEditor : Editor
{
    SerializedProperty hasWarmDamage, warmChance;
    SerializedProperty hasBurnDamage, burnChance;
    SerializedProperty hasStunDamage, stunChance;
    SerializedProperty hasRootDamage, rootChance;
    SerializedProperty hasBleedingDamage, bleedingChance;
    SerializedProperty hasSlowDamage, slowChance;
    SerializedProperty hasPoisonDamage, poisonChance;
    SerializedProperty hasAcidDamage, acidEfficiency;
    SerializedProperty hasStaticDamage, staticChance;
    SerializedProperty hasParalyzeDamage, paralyzeChance;
    SerializedProperty hasChillDamage, chillChance;
    SerializedProperty hasFrostDamage, frostChance;
    SerializedProperty hasBlindDamage, blindChance;
    SerializedProperty hasRevealDamage, revealChance;
    SerializedProperty hasCurseDamage, curseChance;
    SerializedProperty hasFearDamage, fearChance;
    SerializedProperty canDrainHealth, healthDrainChance;

    // Optional: define property ranges here for consistency
    private void OnEnable()
    {
        hasWarmDamage = serializedObject.FindProperty("hasWarmDamage");
        warmChance = serializedObject.FindProperty("warmChance");

        hasBurnDamage = serializedObject.FindProperty("hasBurnDamage");
        burnChance = serializedObject.FindProperty("burnChance");

        hasStunDamage = serializedObject.FindProperty("hasStunDamage");
        stunChance = serializedObject.FindProperty("stunChance");

        hasRootDamage = serializedObject.FindProperty("hasRootDamage");
        rootChance = serializedObject.FindProperty("rootChance");

        hasBleedingDamage = serializedObject.FindProperty("hasBleedingDamage");
        bleedingChance = serializedObject.FindProperty("bleedingChance");

        hasSlowDamage = serializedObject.FindProperty("hasSlowDamage");
        slowChance = serializedObject.FindProperty("slowChance");

        hasPoisonDamage = serializedObject.FindProperty("hasPoisonDamage");
        poisonChance = serializedObject.FindProperty("poisonChance");

        hasAcidDamage = serializedObject.FindProperty("hasAcidDamage");
        acidEfficiency = serializedObject.FindProperty("acidEfficiency");

        hasStaticDamage = serializedObject.FindProperty("hasStaticDamage");
        staticChance = serializedObject.FindProperty("staticChance");

        hasParalyzeDamage = serializedObject.FindProperty("hasParalyzeDamage");
        paralyzeChance = serializedObject.FindProperty("paralyzeChance");

        hasChillDamage = serializedObject.FindProperty("hasChillDamage");
        chillChance = serializedObject.FindProperty("chillChance");

        hasFrostDamage = serializedObject.FindProperty("hasFrostDamage");
        frostChance = serializedObject.FindProperty("frostChance");

        hasBlindDamage = serializedObject.FindProperty("hasBlindDamage");
        blindChance = serializedObject.FindProperty("blindChance");

        hasRevealDamage = serializedObject.FindProperty("hasRevealDamage");
        revealChance = serializedObject.FindProperty("revealChance");

        hasCurseDamage = serializedObject.FindProperty("hasCurseDamage");
        curseChance = serializedObject.FindProperty("curseChance");

        hasFearDamage = serializedObject.FindProperty("hasFearDamage");
        fearChance = serializedObject.FindProperty("fearChance");

        canDrainHealth = serializedObject.FindProperty("canDrainHealth");
        healthDrainChance = serializedObject.FindProperty("healthDrainChance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw everything up to "ATTACK DETAILS"
        DrawPropertiesUpTo("hasWarmDamage");

        EditorGUILayout.PropertyField(hasWarmDamage);
        if (hasWarmDamage.boolValue) EditorGUILayout.PropertyField(warmChance);

        EditorGUILayout.PropertyField(hasBurnDamage);
        if (hasBurnDamage.boolValue) EditorGUILayout.PropertyField(burnChance);

        EditorGUILayout.PropertyField(hasStunDamage);
        if (hasStunDamage.boolValue) EditorGUILayout.PropertyField(stunChance);

        EditorGUILayout.PropertyField(hasRootDamage);
        if (hasRootDamage.boolValue) EditorGUILayout.PropertyField(rootChance);

        EditorGUILayout.PropertyField(hasBleedingDamage);
        if (hasBleedingDamage.boolValue) EditorGUILayout.PropertyField(bleedingChance);

        EditorGUILayout.PropertyField(hasSlowDamage);
        if (hasSlowDamage.boolValue) EditorGUILayout.PropertyField(slowChance);

        EditorGUILayout.PropertyField(hasPoisonDamage);
        if (hasPoisonDamage.boolValue) EditorGUILayout.PropertyField(poisonChance);

        EditorGUILayout.PropertyField(hasAcidDamage);
        if (hasAcidDamage.boolValue) EditorGUILayout.PropertyField(acidEfficiency);

        EditorGUILayout.PropertyField(hasStaticDamage);
        if (hasStaticDamage.boolValue) EditorGUILayout.PropertyField(staticChance);

        EditorGUILayout.PropertyField(hasParalyzeDamage);
        if (hasParalyzeDamage.boolValue) EditorGUILayout.PropertyField(paralyzeChance);

        EditorGUILayout.PropertyField(hasChillDamage);
        if (hasChillDamage.boolValue) EditorGUILayout.PropertyField(chillChance);

        EditorGUILayout.PropertyField(hasFrostDamage);
        if (hasFrostDamage.boolValue) EditorGUILayout.PropertyField(frostChance);

        EditorGUILayout.PropertyField(hasBlindDamage);
        if (hasBlindDamage.boolValue) EditorGUILayout.PropertyField(blindChance);

        EditorGUILayout.PropertyField(hasRevealDamage);
        if (hasRevealDamage.boolValue) EditorGUILayout.PropertyField(revealChance);

        EditorGUILayout.PropertyField(hasCurseDamage);
        if (hasCurseDamage.boolValue) EditorGUILayout.PropertyField(curseChance);

        EditorGUILayout.PropertyField(hasFearDamage);
        if (hasFearDamage.boolValue) EditorGUILayout.PropertyField(fearChance);

        EditorGUILayout.PropertyField(canDrainHealth);
        if (canDrainHealth.boolValue) EditorGUILayout.PropertyField(healthDrainChance);

        // Now draw everything else below "healthDrainChance"
        DrawRemainingProperties(after: "healthDrainChance");

        serializedObject.ApplyModifiedProperties();
    }

    void DrawPropertiesUpTo(string stopAtPropertyName)
    {
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren))
        {
            if (prop.name == stopAtPropertyName) return;

            EditorGUILayout.PropertyField(prop, true);
            enterChildren = false;
        }
    }

    void DrawRemainingProperties(string after)
    {
        bool found = false;
        SerializedProperty prop = serializedObject.GetIterator();
        prop.NextVisible(true); // skip script ref

        while (prop.NextVisible(false))
        {
            if (!found)
            {
                if (prop.name == after)
                    found = true;
                continue;
            }

            //// Skip the ones we handled manually
            //if (prop.name == "hasBurnDamage" || prop.name == "poisonChance" || prop.name == "acidEfficiency" ||
            //    prop.name == "stunChance" || prop.name == "frostChance" || prop.name == "curseChance" || prop.name == "blindChance")
            //    continue;

            EditorGUILayout.PropertyField(prop, true);
        }
    }
}
