#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModeSceneGuard
{
    static PlayModeSceneGuard()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            var active = SceneManager.GetActiveScene().name;

            if (active != "CinematicsScene")
            {
                Debug.LogWarning(
                    $"Play started from '{active}'. " +
                    $"Recommended start scene is 'CinematicsScene'.");
            }
        }
    }
}

#endif
