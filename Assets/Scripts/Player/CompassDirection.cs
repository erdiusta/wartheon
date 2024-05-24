using System;
using UnityEngine;

public class CompassDirection : MonoBehaviour
{
    [HideInInspector] public bool isEnabled;

    SpriteRenderer arrowSpriteRenderer;
    Vector3 playerPosition;
    Vector3 bossRoomPosition;
    Vector3 arrowVector;

    private void Awake()
    {
        arrowSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnCompassEnabled += StaticEventHandler_OnCompassEnabled;
        StaticEventHandler.OnCompassDisabled += StaticEventHandler_OnCompassDisabled;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnCompassEnabled -= StaticEventHandler_OnCompassEnabled;
        StaticEventHandler.OnCompassDisabled -= StaticEventHandler_OnCompassDisabled;
    }

    private void Start()
    {
        arrowSpriteRenderer.enabled = false;

        //bossRoomPosition = GameManager.Instance.GetBossRoom().instantiatedRoom.transform.position;
    }

    private void Update()
    {
        if (isEnabled)
        {
            arrowVector = (GameManager.Instance.GetBossRoom().instantiatedRoom.transform.position - transform.position).normalized;
            transform.eulerAngles = new Vector3(0f, 0f, HelperUtilities.GetAngleFromVector(arrowVector));

            arrowSpriteRenderer.enabled = true;
        }
        else
        {
            arrowSpriteRenderer.enabled = false;
        }
    }

    private void StaticEventHandler_OnCompassEnabled()
    {
        EnableCompass();
    }

    private void StaticEventHandler_OnCompassDisabled()
    {
        DisableCompass();
    }
    private void EnableCompass()
    {
        isEnabled = true;
    }

    private void DisableCompass()
    {
        isEnabled = false;
    }

}
