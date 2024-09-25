using UnityEngine;

[CreateAssetMenu(fileName = "MovementDetails_", menuName = "Scriptable Objects/Movement/Movement Details")]
public class MovementDetailsSO : ScriptableObject
{
    #region Header MOVEMENT DETAILS
    [Space(10)]
    [Header("MOVEMENT DETAILS")]
    #endregion Header
    #region Tooltip
    [Tooltip("The maximum move speed. The GetMoveSpeed method calculates a random value between the minimum and maximum")]
    #endregion Tooltip
    public float moveSpeed = 8f;
    #region Tooltip
    [Tooltip("If there is a roll movement - this is the roll speed")]
    #endregion Tooltip
    public float rollSpeed;
    #region Tooltip
    [Tooltip("If there is a roll movement - this is the roll distance")]
    #endregion Tooltip
    public float rollDistance;
    #region Tooltip
    [Tooltip("If there is a roll movement - this is the cooldown time in seconds between roll actions")]
    #endregion Tooltip
    public float rollCooldownTime;

    /// <summary>
    /// Get a random movement speed between the minimum and maximum values
    /// </summary>
    public float GetMoveSpeed() => moveSpeed;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(moveSpeed), moveSpeed, false);
    }
#endif
    #endregion Validation
}
