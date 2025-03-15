using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BatchSpritePivotChanger : EditorWindow
{
    Vector2 newPivot = new Vector2(0.5f, 0.5f); // Default pivot (center)
    List<Sprite> selectedSprites = new List<Sprite>();
    Vector2 scrollPosition; // Scroll position for the sprite list

    [MenuItem("Tools/Batch Change Sprite Pivots")]
    public static void ShowWindow()
    {
        GetWindow<BatchSpritePivotChanger>("Batch Sprite Pivot");
    }

    private void OnGUI()
    {
        GUILayout.Label("Drag and Drop Sprites Here", EditorStyles.boldLabel);

        // Drag-and-drop area
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop Sprites Here", EditorStyles.helpBox); // Visual appearance for dragged area

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // Visual drag cursor icon

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is Sprite sprite && !selectedSprites.Contains(sprite))
                        {
                            selectedSprites.Add(sprite);
                        }
                        else if (draggedObject is Texture2D texture) // Handle dragged parent texture
                        {
                            string path = AssetDatabase.GetAssetPath(texture);
                            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

                            foreach (Object asset in assets)
                            {
                                if (asset is Sprite texSprite && !selectedSprites.Contains(texSprite))
                                {
                                    selectedSprites.Add(texSprite);
                                }
                            }
                        }
                    }

                    Event.current.Use(); // To avoid conflicts and ensure the event is handled only by this too
                }
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Selected Sprites", EditorStyles.boldLabel);

        // Start scroll view
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

        for (int i = 0; i < selectedSprites.Count; i++)
        {
            selectedSprites[i] = (Sprite)EditorGUILayout.ObjectField(selectedSprites[i], typeof(Sprite), false);
        }

        GUILayout.EndScrollView(); // End scroll view

        if (GUILayout.Button("Clear List"))
        {
            selectedSprites.Clear();
        }

        GUILayout.Space(10);
        GUILayout.Label("Set New Pivot", EditorStyles.boldLabel);
        newPivot = EditorGUILayout.Vector2Field("Pivot:", newPivot);

        if (GUILayout.Button("Apply Pivot to Listed Sprites"))
        {
            ChangePivotForSprites();
        }
    }

    private void ChangePivotForSprites()
    {
        foreach (Sprite sprite in selectedSprites)
        {
            if (sprite == null) continue;

            string path = AssetDatabase.GetAssetPath(sprite.texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null && importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                SerializedObject so = new SerializedObject(importer);
                SerializedProperty sprites = so.FindProperty("m_SpriteSheet.m_Sprites");

                bool modified = false;

                for (int i = 0; i < sprites.arraySize; i++)
                {
                    SerializedProperty spriteElement = sprites.GetArrayElementAtIndex(i);
                    SerializedProperty nameProp = spriteElement.FindPropertyRelative("m_Name");

                    // Process ALL sprites that exist in the batch
                    if (selectedSprites.Exists(s => s.name == nameProp.stringValue))
                    {
                        SerializedProperty pivot = spriteElement.FindPropertyRelative("m_Pivot");
                        SerializedProperty alignment = spriteElement.FindPropertyRelative("m_Alignment");

                        pivot.vector2Value = newPivot;
                        alignment.intValue = 9; // Force "Custom Pivot" mode
                        modified = true;
                    }
                }

                if (modified) // Apply changes only if something was modified
                {
                    so.ApplyModifiedProperties();
                    importer.SaveAndReimport();
                }
            }
        }

        Debug.Log("Pivot updated for all selected sprites.");
    }
}
