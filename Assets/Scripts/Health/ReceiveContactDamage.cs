using UnityEngine;

[RequireComponent(typeof(Health))]
[DisallowMultipleComponent]
public class ReceiveContactDamage : MonoBehaviour
{
    #region Header
    [Header("The contact damage amount to receive")]
    #endregion
    [SerializeField] int contactDamageAmount;

    Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public void TakeContactDamage(int damageAmount, Vector2 dealerPosition, Vector2 receiverPosition)
    {
        if (GetComponent<Enemy>() == null)
        {
            if (contactDamageAmount > 0) damageAmount = contactDamageAmount;
        }

        health.TakeDamage(damageAmount, dealerPosition, receiverPosition);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(contactDamageAmount), contactDamageAmount, true);
    }
#endif
    #endregion
}