using UnityEngine;

public class Decoy : MonoBehaviour
{
    public ActiveItemDetailsSO activeItemDetails;
    public Health health;

    [HideInInspector] public SpriteRenderer spriteRenderer;

    ActiveItem activeItem;

    private void Awake()
    {
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        health.SetMaximumHealth(30);
    }

    public ActiveItem InitializeDecoy()
    {
        activeItem = new ActiveItem
        {
            activeItemDetails = activeItemDetails,
            activeItemRemainingCharge = activeItemDetails.activeItemMaxCharge
        };

        return activeItem;
    }

    public Vector3 GetDecoyPosition() => transform.position;
}
