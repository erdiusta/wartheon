using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using System.Reflection;
using System;

public class AnimationClipSwitcher : EditorWindow
{
    Animator selectedAnimator;
    AnimationClip[] animationClips;
    Vector2 scrollPos;
    AnimationClip lastSelectedClip; // Store the last selected clip

    [MenuItem("Tools/Animation Clip Switcher")]
    public static void ShowWindow()
    {
        GetWindow<AnimationClipSwitcher>("Animation Clip Switcher");
    }

    private void OnGUI()
    {
        // Select Animator
        EditorGUILayout.Space(10);
        selectedAnimator = EditorGUILayout.ObjectField("Animator", selectedAnimator, typeof(Animator), true) as Animator;

        if (selectedAnimator == null)
        {
            EditorGUILayout.HelpBox("Select an Animator to display its animation clips.", MessageType.Info);
            return;
        }
        else
        {
            animationClips = GetAnimationClips(selectedAnimator);
        }

        if (animationClips == null || animationClips.Length == 0)
        {
            EditorGUILayout.HelpBox("No Animation Clips found.", MessageType.Warning);
            return;
        }

        // Scrollable clip list
        EditorGUILayout.Space(10);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(500));

        foreach (var clip in animationClips)
        {
            GUI.backgroundColor = (clip == lastSelectedClip) ? Color.red : Color.white; // Highlight last selected clip

            if (GUILayout.Button(clip.name))
            {
                SelectAnimationClip(clip);
            }
        }

        GUI.backgroundColor = Color.white; // Reset button color
        EditorGUILayout.EndScrollView();
    }

    private AnimationClip[] GetAnimationClips(Animator animator)
    {
        var runtimeAnimatorController = animator.runtimeAnimatorController;
        if (runtimeAnimatorController is AnimatorController)
        {
            return runtimeAnimatorController.animationClips;
        }
        return new AnimationClip[0];
    }

    private void SelectAnimationClip(AnimationClip clip)
    {
        if(clip == null || selectedAnimator == null)
        {
            Debug.LogError("No animation clip or animator selected!");
            return;
        }

        lastSelectedClip = clip; // Store selected clip

        // Set active object with context (forces Animation Window to recognize the selection)
        Selection.SetActiveObjectWithContext(clip, clip);

        // Select the gameObject in the hierarchy
        Selection.activeGameObject = selectedAnimator.gameObject;

        // Open animation window
        EditorApplication.ExecuteMenuItem("Window/Animation/Animation");

        // Ensure Unity refreshes before setting the clip
        EditorApplication.delayCall += () =>
        {
            // Get the Animation Window
            Type animationWindowType = typeof(Editor).Assembly.GetType("UnityEditor.AnimationWindow");
            EditorWindow animationWindow = GetWindow(animationWindowType);
            if (animationWindow == null)
            {
                Debug.LogError("Animation Window could not be opened.");
                return;
            }

            // Get Animation Window's internal state
            FieldInfo stateField = animationWindowType.GetField("m_AnimEditor", BindingFlags.Instance | BindingFlags.NonPublic);
            var animEditor = stateField?.GetValue(animationWindow);
            if (animEditor == null)
            {
                Debug.LogError("Could not access Animation Window's AnimEditor.");
                return;
            }

            // Access AnimationWindowState
            PropertyInfo stateProperty = animEditor.GetType().GetProperty("state", BindingFlags.Instance | BindingFlags.Public);
            var animationState = stateProperty?.GetValue(animEditor);
            if (animationState == null)
            {
                Debug.LogError("Could not access Animation Window state.");
                return;
            }

            // Try setting the active animation clip
            PropertyInfo activeClipProperty = animationState.GetType().GetProperty("activeAnimationClip", BindingFlags.Instance | BindingFlags.Public);
            if (activeClipProperty != null)
            {
                activeClipProperty.SetValue(animationState, clip);
            }
            else
            {
                Debug.LogError("Could not find activeAnimationClip property.");
            }
        };
    }
}