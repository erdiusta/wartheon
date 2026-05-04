using UnityEngine;

[DisallowMultipleComponent]
public class Environment : MonoBehaviour
{
    // Attach this class to environment game objects whose lighting gets in
    #region Header References
    [Space(10)]
    [Header("References")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the SpriteRenderer component on the prefab")]
    #endregion
    public SpriteRenderer spriteRenderer;
}
