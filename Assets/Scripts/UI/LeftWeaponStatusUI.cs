using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeftWeaponStatusUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion Header OBJECT REFERENCES
    #region Tooltip
    [Tooltip("Populate with image component on the child WeaponImage gameobject")]
    #endregion Tooltip
    [SerializeField] Image weaponImage;
    #region Tooltip
    [Tooltip("Populate with blank 1x1 as no weapon sprite on the child WeaponImage gameobject")]
    #endregion Tooltip
    [SerializeField] Sprite noWeaponSprite;
    #region Tooltip
    [Tooltip("Populate with the TextMeshPro-Text component on the child WeaponNameText gameobject")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI weaponNameText;

    Player player;

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        player.setActiveWeaponEvent.OnSetActiveLeftHandWeapon += SetActiveWeaponEvent_OnSetActiveLeftHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveLeftHandWeapon += SetActiveWeaponEvent_OnSetInactiveLeftHandWeapon;
    }

    private void OnDisable()
    {
        player.setActiveWeaponEvent.OnSetActiveLeftHandWeapon -= SetActiveWeaponEvent_OnSetActiveLeftHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveLeftHandWeapon -= SetActiveWeaponEvent_OnSetInactiveLeftHandWeapon;
    }

    private void Start()
    {
        // Update active weapon status on the UI
        SetActiveWeapon(player.activeWeapon.GetCurrentLeftHandWeapon());
    }

    /// <summary>
    /// Handle set active weapon event on the UI
    /// </summary>
    private void SetActiveWeaponEvent_OnSetActiveLeftHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetActiveWeapon(setActiveWeaponEventArgs.weapon);
    }

    /// <summary>
    /// Clear active weapon event on the UI
    /// </summary>
    private void SetActiveWeaponEvent_OnSetInactiveLeftHandWeapon(SetActiveWeaponEvent @event)
    {
        MakeWeaponInactive();
    }

    /// <summary>
    /// Set the active weapon on the UI
    /// </summary>
    private void SetActiveWeapon(Weapon weapon)
    {
        if (weapon != null)
        {
            UpdateActiveWeaponImage(weapon.weaponDetails);
            UpdateActiveWeaponName(weapon);
        }
    }

    /// <summary>
    /// Populate active weapon image
    /// </summary>
    private void UpdateActiveWeaponImage(WeaponDetailsSO weaponDetails)
    {
        weaponImage.sprite = weaponDetails.weaponFrontSprite;
    }

    /// <summary>
    /// Populate active weapon name
    private void UpdateActiveWeaponName(Weapon weapon)
    {
        weaponNameText.text = weapon.weaponDetails.weaponName;
    }

    private void MakeWeaponInactive()
    {
        weaponImage.sprite = noWeaponSprite;
        weaponNameText.text = "";
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponImage), weaponImage);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponNameText), weaponNameText);
    }
#endif
    #endregion
}