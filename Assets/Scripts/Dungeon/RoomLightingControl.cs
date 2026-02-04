using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(InstantiatedRoom))]
[DisallowMultipleComponent]
public class RoomLightingControl : MonoBehaviour
{
    InstantiatedRoom instantiatedRoom;

    Player player;

    private void Awake()
    {
        instantiatedRoom = GetComponent<InstantiatedRoom>();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        if (roomChangedEventArgs.room != null)
        {
            // If this is the room entered and the room isn't already lit, then fade in the room lighting
            if (roomChangedEventArgs.room == instantiatedRoom.room && !instantiatedRoom.room.isLit)
            {
                if (!roomChangedEventArgs.room.roomNodeType.isEntrance)
                {
                    Lighting(instantiatedRoom);
                }
                else
                {
                    // Ensure room environment decoration game objects are activated
                    instantiatedRoom.ActivateEnvironmentGameObjects();
                }

                instantiatedRoom.room.isLit = true;
            }
            return;
        }

        if (roomChangedEventArgs.roomNetData == instantiatedRoom.roomNetData && !instantiatedRoom.roomNetData.isLit)
        {
            if (!roomChangedEventArgs.roomNetData.isEntrance)
            {
                Lighting(instantiatedRoom);
            }
            else
            {
                instantiatedRoom.ActivateEnvironmentGameObjects();
            }

            instantiatedRoom.roomNetData.isLit = true;
        }
    }

    private void Lighting(InstantiatedRoom instantiatedRoom)
    {
        // Fade in room
        FadeInRoomLighting();

        // Ensure room environment decoration game objects are activated
        instantiatedRoom.ActivateEnvironmentGameObjects();

        // Fade in the environment decoration gameobjects lighting
        FadeInEnvironmentLighting();

        // Fade in the room doors lighting
        FadeInDoors();
    }

    /// <summary>
    /// Fade in the room lighting
    /// </summary>
    private void FadeInRoomLighting()
    {
        // Fade in the lighting for the room tilemaps
        StartCoroutine(FadeInRoomLightingRoutine(instantiatedRoom));
    }

    /// <summary>
    /// Fade in the room lighting coroutine
    /// </summary>
    IEnumerator FadeInRoomLightingRoutine(InstantiatedRoom instantiatedRoom)
    {
        // Create new material to fade in
        Material material = new Material(GameResources.Instance.dimmedMaterial);

        instantiatedRoom.groundTilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.decoration1Tilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.decoration2Tilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.sideTilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.frontTilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.minimapTilemap.GetComponent<TilemapRenderer>().material = material;

        for (float i = 0.05f; i <= 1f; i += Time.deltaTime / Settings.fadeInTime)
        {
            material.SetFloat("Alpha_Slider", i);
            yield return null;
        }

        // Set material back to lit material - ROOM TILEMAPS
        instantiatedRoom.groundTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.decoration1Tilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.decoration2Tilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.sideTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.frontTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.minimapTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;

        // Set material back to lit material - ROOM PROPS & LIGHTS
        foreach (Transform item in instantiatedRoom.environmentGameObject.transform)
        {
            item.GetComponentInChildren<SpriteRenderer>().material = GameResources.Instance.litMaterial;
        }
    }

    /// <summary>
    /// Fade in the environmental decoration game objects
    /// </summary>
    private void FadeInEnvironmentLighting()
    {
        // Create new material to fade in
        Material material = new Material(GameResources.Instance.dimmedMaterial);

        // Get all environment components in room
        Environment[] environmentComponents = GetComponentsInChildren<Environment>();

        // Loop through
        foreach (Environment environmentComponent in environmentComponents)
        {
            if (environmentComponent.spriteRenderer != null)
                environmentComponent.spriteRenderer.material = material;
        }

        StartCoroutine(FadeInEnvironmentLightingRoutine(material, environmentComponents));
    }

    /// <summary>
    /// Fade in the environmental decoration game objects coroutine
    /// </summary>
    IEnumerator FadeInEnvironmentLightingRoutine(Material material, Environment[] environmentComponents)
    {
        // Gradually fade in the lighting
        for (float i = 0.05f; i <= 1f; i += Time.deltaTime / Settings.fadeInTime)
        {
            material.SetFloat("Alpha_Slider", i);
            yield return null;
        }

        // Set environment components material back to lit material
        foreach (Environment environmentComponent in environmentComponents)
        {
            if (environmentComponent.spriteRenderer != null)
                environmentComponent.spriteRenderer.material = GameResources.Instance.litMaterial;
        }
    }

    /// <summary>
    /// Fade in the doors
    /// </summary>
    private void FadeInDoors()
    {
        Door[] doorArray = GetComponentsInChildren<Door>();

        foreach (Door door in doorArray)
        {
            DoorLightingControl doorLightingControl = door.GetComponentInChildren<DoorLightingControl>();
            doorLightingControl.FadeInDoor(door);
        }
    }
}
