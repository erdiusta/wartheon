using Mirror;
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

    public void TakeContactDamage(int damageAmount, DamageContext ctx)
    {
        if (GetComponent<Enemy>() == null && contactDamageAmount > 0) damageAmount = contactDamageAmount;

        GameObject target = TryGetComponent(out RoomProp prop) ? prop.gameObject : null;
        HealthAuthorityResolver.GetAuthority(health.gameObject).ApplyDamage(damageAmount, ctx, target);
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