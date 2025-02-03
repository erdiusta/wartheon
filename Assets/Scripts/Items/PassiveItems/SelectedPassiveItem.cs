using UnityEngine;
using System;

public class SelectedPassiveItem : MonoBehaviour
{
    public bool isHeadEquipped { get; private set; }
    public bool isChestEquipped { get; private set; }
    public bool isNeckEquipped { get; private set; }
    public bool isArmEquipped { get; private set; }
    public bool isFingerEquipped { get; private set; }
    public bool isWaistEquipped { get; private set; }
    public bool isBackEquipped { get; private set; }
    public bool isLegEquipped { get; private set; }

    Player player;

    [HideInInspector] public PassiveItem headPassiveItem;
    [HideInInspector] public PassiveItem chestPassiveItem;
    [HideInInspector] public PassiveItem neckPassiveItem;
    [HideInInspector] public PassiveItem armPassiveItem;
    [HideInInspector] public PassiveItem fingerPassiveItem;
    [HideInInspector] public PassiveItem waistPassiveItem;
    [HideInInspector] public PassiveItem backPassiveItem;
    [HideInInspector] public PassiveItem legPassiveItem;

    SetPassiveItemEvent setPassiveItemEvent;

    private void Awake()
    {
        setPassiveItemEvent = GetComponent<SetPassiveItemEvent>();
    }

    private void OnEnable()
    {
        setPassiveItemEvent.OnEquippedPassiveItem += SetPassiveItemEvent_OnEquippedPassiveItem;
        setPassiveItemEvent.OnRemovedPassiveItem += SetPassiveItemEvent_OnRemovedPassiveItem;
    }

    private void OnDisable()
    {
        setPassiveItemEvent.OnEquippedPassiveItem -= SetPassiveItemEvent_OnEquippedPassiveItem;
        setPassiveItemEvent.OnRemovedPassiveItem -= SetPassiveItemEvent_OnRemovedPassiveItem;
    }

    private void SetPassiveItemEvent_OnEquippedPassiveItem(SetPassiveItemEvent setPassiveItemEvent, SetPassiveItemEventArgs setPassiveItemEventArgs)
    {
        player = GameManager.Instance.GetPlayer();

        switch (setPassiveItemEventArgs.passiveItemSlotName)
        {
            case PassiveItemSlotName.Head:
                if (!isHeadEquipped)
                {
                    headPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HaloOfBlindingRadiance)
                    {
                        player.currentLightResistanceValue += 0.05f;
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue, 2);
                        Debug.Log("Blinding chance increased.");
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HelmOfTheEternalVigil)
                    {
                        Debug.Log("Reduces damage taken by 4 while standing still.");
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EnchantersSpire)
                    {
                        player.currentFireResistanceValue += 0.05f;
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue, 2);
                        player.currentEarthResistanceValue += 0.05f;
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue, 2);
                        player.currentAirResistanceValue += 0.05f;
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue, 2);
                        player.currentWaterResistanceValue += 0.05f;
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue, 2);
                        player.currentLightResistanceValue += 0.05f;
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue, 2);
                        player.currentDarkResistanceValue += 0.05f;
                        player.currentDarkResistanceValue = (float)Math.Round(player.currentDarkResistanceValue, 2);
                        player.currentIntelligenceValue += 1;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WhisperingHood)
                    {
                        player.currentBlockValue += 0.15f;
                        player.currentBlockValue = player.currentBlockValue.HasValue ? (float?)Math.Round(player.currentBlockValue.Value, 2) : null;
                        player.currentDexterityValue += 1;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GildedGuardian)
                    {
                        player.currentPhysicalResistanceValue += 0.2f;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue, 2);
                        player.currentConstitutionValue += 1;
                    }

                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    isHeadEquipped = true;
                }
                break;
            case PassiveItemSlotName.Chest:
                if (!isChestEquipped)
                {
                    chestPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (chestPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ChestplateOfTheLastLight)
                    {
                        player.currentPhysicalResistanceValue += 0.3f;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue, 2);
                        Debug.Log("When below 50% health absorbs 20% of damage.");
                    }

                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    isChestEquipped = true;
                }
                break;
            case PassiveItemSlotName.Neck:
                if (!isNeckEquipped)
                {
                    neckPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RubyPendant)
                    {
                        player.currentFireResistanceValue = player.currentFireResistanceValue + 0.2f;
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EmeraldPendant)
                    {
                        player.currentEarthResistanceValue = player.currentEarthResistanceValue + 0.2f;
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.TopazPendant)
                    {
                        player.currentAirResistanceValue = player.currentAirResistanceValue + 0.2f;
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SapphirePendant)
                    {
                        player.currentWaterResistanceValue = player.currentWaterResistanceValue + 0.2f;
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue, 2);
                    }

                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    isNeckEquipped = true;
                }
                break;
            case PassiveItemSlotName.Finger:
                if (!isFingerEquipped)
                {
                    fingerPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfTempestStrikes)
                    {
                        Debug.Log("Attack speed increases by 20%.");
                        player.currentPhysicalResistanceValue = Mathf.Clamp(player.currentPhysicalResistanceValue - 1f, 0, player.currentPhysicalResistanceValue - 1f);
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue, 2);
                    }

                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    isFingerEquipped = true;
                }
                break;
            case PassiveItemSlotName.Back:
                if (!isBackEquipped)
                {
                    backPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RecantersCloak)
                    {
                        player.currentBlockValue += 0.1f;
                        player.currentBlockValue = player.currentBlockValue.HasValue ? (float?)Math.Round(player.currentBlockValue.Value, 2) : null;
                        player.currentAgilityValue += 1;
                    }

                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    isBackEquipped = true;
                }
                break;
            case PassiveItemSlotName.Waist:
                if (!isWaistEquipped)
                {
                    waistPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BeltOfSorcery)
                    {
                        Debug.Log("CAST SPEED INCREASED.");
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.InfernoSash)
                    {
                        player.currentFireResistanceValue += 0.15f;
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue, 2);

                        player.currentPhysicalResistanceValue += 0.05f;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue, 2);

                        Debug.Log("Every 5th hit causes burn.");
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GirdleOfFirmament)
                    {
                        player.currentAirResistanceValue += 0.05f;
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue, 2);

                        player.currentLightResistanceValue = player.currentLightResistanceValue + 0.05f;
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue, 2);
                        Debug.Log("Immune to blind");
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BloodforgedGirdle)
                    {
                        player.currentMainHandMinDamageValue += 3;
                        player.currentMainHandMaxDamageValue += 3;
                        player.movementByVelocity.moveSpeed += 0.5f;
                        Debug.Log("Attack speed increases.");
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SandweaversSash)
                    {
                        player.currentBlockValue += 0.15f;
                        Debug.Log("+1 Physical damage per successful hit (max stack +8)");

                    }

                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    isWaistEquipped = true;
                }
                break;
            case PassiveItemSlotName.Arm:
                if (!isArmEquipped)
                {
                    armPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (armPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.OminousGripOfThunder)
                    {
                        player.currentPhysicalResistanceValue += 0.05f ;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue, 2);
                        player.currentAirResistanceValue += 0.1f;
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue, 2);
                    }

                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    isArmEquipped = true;
                }
                break;
            case PassiveItemSlotName.Leg:
                if (!isLegEquipped)
                {
                    legPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (legPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WingedSandals)
                    {
                        player.movementByVelocity.moveSpeed += 1f;
                    }

                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    isLegEquipped = true;
                }
                break;
            default:
                break;
        }
    }

    private void SetPassiveItemEvent_OnRemovedPassiveItem(SetPassiveItemEvent setPassiveItemEvent, SetPassiveItemEventArgs setPassiveItemEventArgs)
    {
        player = GameManager.Instance.GetPlayer();

        switch (setPassiveItemEventArgs.passiveItemSlotName)
        {
            case PassiveItemSlotName.Head:
                if (isHeadEquipped)
                {
                    if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HaloOfBlindingRadiance)
                    {
                        player.currentLightResistanceValue -= 0.05f;
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue, 2);
                        Debug.Log("Blinding chance increased.");
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HelmOfTheEternalVigil)
                    {
                        Debug.Log("Reduces damage taken by 4 while standing still.");
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EnchantersSpire)
                    {
                        player.currentFireResistanceValue -= 0.05f;
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue, 2);
                        player.currentEarthResistanceValue -= 0.05f;
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue, 2);
                        player.currentAirResistanceValue -= 0.05f;
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue, 2);
                        player.currentWaterResistanceValue -= 0.05f;
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue, 2);
                        player.currentLightResistanceValue -= 0.05f;
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue, 2);
                        player.currentDarkResistanceValue -= 0.05f;
                        player.currentDarkResistanceValue = (float)Math.Round(player.currentDarkResistanceValue, 2);
                        player.currentIntelligenceValue -= 1;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WhisperingHood)
                    {
                        player.currentBlockValue -= 0.15f;
                        player.currentBlockValue = player.currentBlockValue.HasValue ? (float?)Math.Round(player.currentBlockValue.Value, 2) : null;
                        player.currentDexterityValue -= 1;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GildedGuardian)
                    {
                        player.currentPhysicalResistanceValue = Mathf.Clamp(player.currentPhysicalResistanceValue - 0.2f, 0f, 1f);
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue, 2);
                        player.currentConstitutionValue -= 1;
                    }

                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    headPassiveItem = null;
                    isHeadEquipped = false;
                }
                break;
            case PassiveItemSlotName.Chest:
                if (isChestEquipped)
                {
                    if (chestPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ChestplateOfTheLastLight)
                    {
                        player.currentPhysicalResistanceValue = Mathf.Clamp(player.currentPhysicalResistanceValue - 0.3f, 0f, 1f);
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue, 2);

                        Debug.Log("When below 50% health absorbs 20% of damage.");
                    }

                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    chestPassiveItem = null;
                    isChestEquipped = false;
                }
                break;
            case PassiveItemSlotName.Neck:
                if (isNeckEquipped)
                {
                    if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RubyPendant)
                    {
                        player.currentFireResistanceValue = player.currentFireResistanceValue - 0.2f;
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EmeraldPendant)
                    {
                        player.currentEarthResistanceValue = player.currentEarthResistanceValue - 0.2f;
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.TopazPendant)
                    {
                        player.currentAirResistanceValue = player.currentAirResistanceValue - 0.2f;
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SapphirePendant)
                    {
                        player.currentWaterResistanceValue = player.currentWaterResistanceValue - 0.2f;
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue, 2);
                    }

                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    neckPassiveItem = null;
                    isNeckEquipped = false;
                }
                break;
            case PassiveItemSlotName.Finger:
                if (isFingerEquipped)
                {
                    if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfTempestStrikes)
                    {
                        Debug.Log("Attack speed increases by 20%.");
                        player.currentPhysicalResistanceValue += 1;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue, 2);
                    }

                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    fingerPassiveItem = null;
                    isFingerEquipped = false;
                }
                break;
            case PassiveItemSlotName.Back:
                if (isBackEquipped)
                {
                    if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RecantersCloak)
                    {
                        player.currentBlockValue -= 0.1f;
                        player.currentBlockValue = player.currentBlockValue.HasValue ? (float?)Math.Round(player.currentBlockValue.Value, 2) : null;
                        player.currentAgilityValue -= 1;
                    }

                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    backPassiveItem = null;
                    isBackEquipped = false;
                }
                break;
            case PassiveItemSlotName.Waist:
                if (isWaistEquipped)
                {
                    if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BeltOfSorcery)
                    {
                        Debug.Log("CAST SPEED INCREASED.");
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.InfernoSash)
                    {
                        player.currentFireResistanceValue -= 0.15f;
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue, 2);

                        player.currentPhysicalResistanceValue -= 0.05f;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue, 2);

                        Debug.Log("Every 5th hit causes burn.");
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GirdleOfFirmament)
                    {
                        player.currentAirResistanceValue = player.currentAirResistanceValue - 0.05f;
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue, 2);
                        player.currentLightResistanceValue = player.currentLightResistanceValue - 0.05f;
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue, 2);
                        Debug.Log("Immune to blind");
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BloodforgedGirdle)
                    {
                        player.currentMainHandMinDamageValue -= 3;
                        player.currentMainHandMaxDamageValue -= 3;
                        player.movementByVelocity.moveSpeed -= 0.5f;
                        Debug.Log("Attack speed increases.");
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SandweaversSash)
                    {
                        player.currentBlockValue -= 0.15f;
                        Debug.Log("+1 Physical damage per successful hit (max stack +8)");

                    }

                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    waistPassiveItem = null;
                    isWaistEquipped = false;
                }
                break;
            case PassiveItemSlotName.Arm:
                if (isArmEquipped)
                {
                    if (armPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.OminousGripOfThunder)
                    {
                        player.currentPhysicalResistanceValue -= 0.05f;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue, 2);
                        player.currentAirResistanceValue -= 0.1f;
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue, 2);
                    }

                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    armPassiveItem = null;
                    isArmEquipped = false;
                }
                break;
            case PassiveItemSlotName.Leg:
                if (isLegEquipped)
                {
                    if (legPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WingedSandals)
                    {
                        player.movementByVelocity.moveSpeed = player.movementByVelocity.playerStartingSpeed;
                    }

                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    legPassiveItem = null;
                    isLegEquipped = false;
                }
                break;
            default:
                break;
        }
    }

    public PassiveItem GetCurrentHeadPassiveItem()
    {
        return headPassiveItem;
    }

    public PassiveItem GetCurrentChestPassiveItem()
    {
        return chestPassiveItem;
    }

    public PassiveItem GetCurrentNeckPassiveItem()
    {
        return neckPassiveItem;
    }

    public PassiveItem GetCurrentArmPassiveItem()
    {
        return armPassiveItem;
    }

    public PassiveItem GetCurrentFingerPassiveItem()
    {
        return fingerPassiveItem;
    }

    public PassiveItem GetCurrentWaistPassiveItem()
    {
        return waistPassiveItem;
    }

    public PassiveItem GetCurrentBackPassiveItem()
    {
        return backPassiveItem;
    }

    public PassiveItem GetCurrentLegPassiveItem()
    {
        return legPassiveItem;
    }
}
