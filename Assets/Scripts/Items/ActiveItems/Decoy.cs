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
        health.SetStartingHealth(30);
    }

    public ActiveItem InitializeDecoy()
    {
        activeItem = new ActiveItem
        {
            activeItemDetails = activeItemDetails,
            activeItemRemainingProjectile = activeItemDetails.activeItemProjectileCapacity
        };

        return activeItem;
    }

    public Vector3 GetDecoyPosition() => transform.position;
}
