using TMPro;
using UnityEngine;

public class ChestItem : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    TextMeshPro textTMP;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        textTMP = GetComponentInChildren<TextMeshPro>();
    }

    public void Initialize(Sprite sprite, string text, Vector3 spawnPosition)
    {
        spriteRenderer.sprite = sprite;
        transform.position = spawnPosition;

        textTMP.text = text;
    }
}
