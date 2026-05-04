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

    public void TakeContactDamage(int damageAmount, DamageContext ctx, Player player = null)
    {
        if (GetComponent<Enemy>() == null && contactDamageAmount > 0) damageAmount = contactDamageAmount;

        HealthAuthorityResolver.GetAuthority(gameObject).ApplyDamage(damageAmount, ctx);
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