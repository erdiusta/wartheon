using UnityEditor;

[CustomEditor(typeof(EnemyDetailsSO))]
public class EnemyDetailsSOEditor : Editor
{
    SerializedProperty dealtMeleeDamageMin, dealtMeleeDamageMax;
    SerializedProperty canBurn, burnChance;
    SerializedProperty isPoisonous, poisonChance;
    SerializedProperty hasAcid, acidEfficiency;
    SerializedProperty hasStunDamage, stunChance;
    SerializedProperty hasFrostDamage, frostChance;
    SerializedProperty hasCurseDamage, curseChance;
    SerializedProperty hasBlindDamage, blindChance;

    // Optional: define property ranges here for consistency
    void OnEnable()
    {
        dealtMeleeDamageMin = serializedObject.FindProperty("dealtMeleeDamageMin");
        dealtMeleeDamageMax = serializedObject.FindProperty("dealtMeleeDamageMax");

        canBurn = serializedObject.FindProperty("canBurn");
        burnChance = serializedObject.FindProperty("burnChance");

        isPoisonous = serializedObject.FindProperty("isPoisonous");
        poisonChance = serializedObject.FindProperty("poisonChance");

        hasAcid = serializedObject.FindProperty("hasAcid");
        acidEfficiency = serializedObject.FindProperty("acidEfficiency");

        hasStunDamage = serializedObject.FindProperty("hasStunDamage");
        stunChance = serializedObject.FindProperty("stunChance");

        hasFrostDamage = serializedObject.FindProperty("hasFrostDamage");
        frostChance = serializedObject.FindProperty("frostChance");

        hasCurseDamage = serializedObject.FindProperty("hasCurseDamage");
        curseChance = serializedObject.FindProperty("curseChance");

        hasBlindDamage = serializedObject.FindProperty("hasBlindDamage");
        blindChance = serializedObject.FindProperty("blindChance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw everything up to "ATTACK DETAILS"
        DrawPropertiesUpTo("dealtMeleeDamageMin");

        EditorGUILayout.PropertyField(dealtMeleeDamageMin);
        EditorGUILayout.PropertyField(dealtMeleeDamageMax);

        EditorGUILayout.PropertyField(canBurn);
        if (canBurn.boolValue) EditorGUILayout.PropertyField(burnChance);

        EditorGUILayout.PropertyField(isPoisonous);
        if (isPoisonous.boolValue) EditorGUILayout.PropertyField(poisonChance);

        EditorGUILayout.PropertyField(hasAcid);
        if (hasAcid.boolValue) EditorGUILayout.PropertyField(acidEfficiency);

        EditorGUILayout.PropertyField(hasStunDamage);
        if (hasStunDamage.boolValue) EditorGUILayout.PropertyField(stunChance);

        EditorGUILayout.PropertyField(hasFrostDamage);
        if (hasFrostDamage.boolValue) EditorGUILayout.PropertyField(frostChance);

        EditorGUILayout.PropertyField(hasCurseDamage);
        if (hasCurseDamage.boolValue) EditorGUILayout.PropertyField(curseChance);

        EditorGUILayout.PropertyField(hasBlindDamage);
        if (hasBlindDamage.boolValue) EditorGUILayout.PropertyField(blindChance);

        // Now draw everything else below "hasBlindDamage"
        DrawRemainingProperties(after: "blindChance");

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

            // Skip the ones we handled manually
            if (prop.name == "burnChance" || prop.name == "poisonChance" || prop.name == "acidEfficiency" ||
                prop.name == "stunChance" || prop.name == "frostChance" || prop.name == "curseChance" || prop.name == "blindChance")
                continue;

            EditorGUILayout.PropertyField(prop, true);
        }
    }
}