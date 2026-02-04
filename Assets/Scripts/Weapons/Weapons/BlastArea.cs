using UnityEngine;
using System.Collections;

public class BlastArea : MonoBehaviour
{
    [SerializeField] CircleOrigin circleOrigin;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] SpriteMask spriteMask;
    [SerializeField] Animator burnAnimator;

    private void Start()
    {
        SetBlastAreaTiling();
    }

    public void SetBlastAreaTiling()
    {
        if (spriteRenderer == null || circleOrigin == null || spriteMask == null) return;

        // Step 1: Calculate the diameter based on the circle's radius
        float diameter = circleOrigin.circleRadius * 2;  // Total diameter, this is the area to cover.

        // Step 2: Set the size of the sprite renderer, ensuring it tiles correctly within this area
        // SpriteRenderer size should match the blast area. Tiling will be controlled by size.
        StartCoroutine(AdjustSpriteSize(diameter));

        // Step 3: Scale the mask to match the sprite size. The mask's transform scale should reflect the same area
        // Match the size of the mask to the sprite, since the mask should clip the outer area of the blast.
        spriteMask.transform.localScale = new Vector3(diameter / spriteMask.sprite.bounds.size.x, diameter / spriteMask.sprite.bounds.size.y, 1f);

        // Step 4: Position the mask to align with the sprite
        // Align the mask's position with the sprite's position in the world
        spriteMask.transform.position = spriteRenderer.transform.position;
    }

    IEnumerator AdjustSpriteSize(float diameter)
    {
        yield return new WaitForEndOfFrame();

        spriteRenderer.size = new Vector2(diameter, diameter);
    }

    public void TriggerBurnAnimation()
    {
        // Trigger the burn animation separately, without affecting tiling
        burnAnimator.SetTrigger("burn");
    }

    private void OnDrawGizmosSelected()
    {
        if (circleOrigin == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, circleOrigin.circleRadius);
    }
}
