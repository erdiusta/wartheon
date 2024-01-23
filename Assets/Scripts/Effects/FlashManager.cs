using UnityEngine;

public class FlashManager : MonoBehaviour
{
    [SerializeField] Material originalMaterial;
    [SerializeField] Material whiteFlashMaterial;
    [SerializeField] Material redFlashMaterial;
    [SerializeField] Material poisonMaterial;

    public void FlashCharacter(SpriteRenderer sprite, Material material)
    {
        sprite.material = material;
    }

    public void WhiteFlashCharacter(SpriteRenderer sprite)
    {
        sprite.material = whiteFlashMaterial;
    }

    public void RedFlashCharacter(SpriteRenderer sprite)
    {
        sprite.material = redFlashMaterial;
    }

    public void PoisonFlashCharacter(SpriteRenderer sprite)
    {
        sprite.material = poisonMaterial;
    }

    public void UnflashCharacter(SpriteRenderer sprite)
    {
        sprite.material = originalMaterial;
    }
}
