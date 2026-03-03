using System.Collections;
using UnityEngine;

public class MaterializeEffect : MonoBehaviour
{
    [Header("Materials")]
    public Shader materializeShader;
    public Material normalMaterial;

    /// <summary>
    /// Materialize effect coroutine - used for the materialiuse special effect
    /// </summary>
    public IEnumerator MaterializeRoutine(Color materializeColor, float materializeTime, SpriteRenderer[] spriteRendererArray)
    {
        Material materializeMaterial = new Material(materializeShader);
        materializeMaterial.SetColor("_EmissionColor", materializeColor);

        // Set materialize material in sprite renderers
        foreach (SpriteRenderer spriteRenderer in spriteRendererArray)
        {
            spriteRenderer.material = materializeMaterial;
        }

        float dissolveAmount = 0f;

        // Materialize enemy
        while (dissolveAmount < 1f)
        {
            dissolveAmount += Time.deltaTime / materializeTime;
            materializeMaterial.SetFloat("_DissolveAmount", dissolveAmount);

            yield return null;
        }

        // Set standard material in sprite renderers
        foreach (SpriteRenderer spriteRenderer in spriteRendererArray)
        {
            spriteRenderer.material = normalMaterial;
        }
    }
}
