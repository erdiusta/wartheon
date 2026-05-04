using UnityEngine;
using UnityEditor;

public static class RemoveMissingScriptsTool
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts (Selected)")]
    static void RemoveMissingScripts()
    {
        foreach (var obj in Selection.gameObjects)
        {
            RemoveRecursively(obj);
        }

        Debug.Log("Missing scripts removed from Selected objects.");
    }

    static void RemoveRecursively(GameObject go)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

        foreach (Transform child in go.transform)
        {
            RemoveRecursively(child.gameObject);
        }
    }
}
