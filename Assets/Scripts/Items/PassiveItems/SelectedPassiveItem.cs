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

                    if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WardenOfForest)
                    {
                        player.additionalBowAccuracyModifier += 0.3f;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HaloOfBlindingRadiance)
                    {
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue + 0.1f, 2);
                        player.additionalBlindMakerModifier += 0.1f;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HelmOfTheEternalVigil)
                    {
                        player.CurrentDexterityValue++;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.1f, 2);
                        player.additionalEvasivenessModifier += 0.1f;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EnchantersSpire)
                    {
                        player.CurrentIntelligenceValue++;
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.05f, 2);
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue + 0.05f, 2);
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.05f, 2);
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue + 0.05f, 2);
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue + 0.05f, 2);
                        player.currentDarkResistanceValue = (float)Math.Round(player.currentDarkResistanceValue + 0.05f, 2);
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WhisperingHood)
                    {
                        player.CurrentDexterityValue++;
                        player.additionalEvasivenessModifier += 0.15f;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GildedGuardian)
                    {
                        player.CurrentConstitutionValue++;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.2f, 2);
                    }

                    isHeadEquipped = true;
                }
                else
                {
                    // Head slot is full, equip in inventory slot if there is room
                    if (!InventoryManager.Instance.IsInventoryFull() && !setPassiveItemEventArgs.isSwap)
                    {
                        setPassiveItemEventArgs.equipResult.placedIntoInventory = true;
                    }
                }
                break;
            case PassiveItemSlotName.Chest:
                if (!isChestEquipped)
                {
                    chestPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (chestPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ChestplateOfTheLastLight)
                    {
                        player.CurrentStrengthValue++;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.3f, 2);
                    }
                    else if (chestPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BlazingHeartplate)
                    {
                        player.CurrentStrengthValue++;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.2f, 2);
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.1f, 2);
                        player.additionalBowAttackCoolDownModifier -= 0.05f;
                        player.additionalMeleeAttackCoolDownModifier -= 0.05f;
                    }
                    else if (chestPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.FrostboundChainmail)
                    {
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.15f, 2);
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue + 0.1f, 2);
                        player.isImmunetoFrost = true;
                    }
                    else if (chestPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.VenomweaveVest)
                    {
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.1f, 2);
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue + 0.1f, 2);
                        player.isImmunetoPoison = true;
                    }

                    isChestEquipped = true;
                }
                else
                {
                    // Chest slot is full, equip in inventory slot if there is room
                    if (!InventoryManager.Instance.IsInventoryFull() && !setPassiveItemEventArgs.isSwap)
                    {
                        setPassiveItemEventArgs.equipResult.placedIntoInventory = true;
                    }
                }
                break;
            case PassiveItemSlotName.Neck:
                if (!isNeckEquipped)
                {
                    neckPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RubyPendant)
                    {
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.2f, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EmeraldPendant)
                    {
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue + 0.2f, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.TopazPendant)
                    {
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.2f, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SapphirePendant)
                    {
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue + 0.2f, 2);
                    }

                    isNeckEquipped = true;
                }
                else
                {
                    // Neck slot is full, equip in inventory slot if there is room
                    if (!InventoryManager.Instance.IsInventoryFull() && !setPassiveItemEventArgs.isSwap)
                    {
                        setPassiveItemEventArgs.equipResult.placedIntoInventory = true;
                    }
                }
                break;
            case PassiveItemSlotName.Finger:
                if (!isFingerEquipped)
                {
                    fingerPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfTempestStrikes)
                    {
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.1f, 2);
                        player.additionalBowAttackCoolDownModifier -= 0.2f;
                        player.additionalMeleeAttackCoolDownModifier -= 0.2f;
                    }
                    else if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfMight)
                    {
                        player.CurrentStrengthValue++;
                    }
                    else if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfVitality)
                    {
                        player.CurrentConstitutionValue++;
                    }
                    else if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfSagacity)
                    {
                        player.CurrentIntelligenceValue++;
                    }
                    else if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfFortune)
                    {
                        player.additionalDropChanceModifier += 0.15f;
                    }

                    isFingerEquipped = true;
                }
                else
                {
                    // Finger slot is full, equip in inventory slot if there is room
                    if (!InventoryManager.Instance.IsInventoryFull() && !setPassiveItemEventArgs.isSwap)
                    {
                        setPassiveItemEventArgs.equipResult.placedIntoInventory = true;
                    }
                }
                break;
            case PassiveItemSlotName.Back:
                if (!isBackEquipped)
                {
                    backPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                    {
                        player.additionalMeleeCriticalHitChanceModifier = (float)Math.Round(player.additionalMeleeCriticalHitChanceModifier + 0.05f, 2);
                        player.shadowCloakEquipped = true;
                    }
                    else if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RecantersCloak)
                    {
                        player.CurrentAgilityValue++;
                        player.additionalEvasivenessModifier += 0.1f;
                    }
                    else if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.MantleOfStars)
                    {
                        player.additionalElementalDamageModifier += 0.05f;
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.15f, 2);
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue + 0.15f, 2);
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.15f, 2);
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue + 0.15f, 2);
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue + 0.15f, 2);
                        player.currentDarkResistanceValue = (float)Math.Round(player.currentDarkResistanceValue + 0.15f, 2);
                    }
                    else if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.CloakOfWindwalker)
                    {
                        player.CurrentAgilityValue += 2;
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.3f, 2);
                    }
                    else if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GoldenCloak)
                    {
                        player.CurrentStrengthValue++;
                        player.CurrentDexterityValue++;
                        player.CurrentConstitutionValue++;
                        player.CurrentIntelligenceValue++;
                        player.CurrentAgilityValue++;
                    }

                    isBackEquipped = true;
                }
                else
                {
                    // Back slot is full, equip in inventory slot if there is room
                    if (!InventoryManager.Instance.IsInventoryFull() && !setPassiveItemEventArgs.isSwap)
                    {
                        setPassiveItemEventArgs.equipResult.placedIntoInventory = true;
                    }
                }
                break;
            case PassiveItemSlotName.Waist:
                if (!isWaistEquipped)
                {
                    waistPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BeltOfSorcery)
                    {
                        player.CurrentIntelligenceValue++;
                        player.additionalCastDurationModifier -= 0.2f;
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.InfernoSash)
                    {
                        player.CurrentConstitutionValue++;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.05f, 2);
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.15f, 2);
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GirdleOfFirmament)
                    {
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.1f, 2);
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue + 0.1f, 2);
                        player.isImmunetoBlind = true;
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BloodforgedGirdle)
                    {
                        player.CurrentStrengthValue++;
                        player.CurrentAgilityValue++;
                        player.additionalMeleeAttackCoolDownModifier -= 0.05f;
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SandweaversSash)
                    {
                        player.CurrentDexterityValue++;
                        player.additionalEvasivenessModifier += 0.1f;
                        player.additionalMeleeCriticalHitChanceModifier += 0.05f;
                    }

                    isWaistEquipped = true;
                }
                else
                {
                    // Waist slot is full, equip in inventory slot if there is room
                    if (!InventoryManager.Instance.IsInventoryFull() && !setPassiveItemEventArgs.isSwap)
                    {
                        setPassiveItemEventArgs.equipResult.placedIntoInventory = true;
                    }
                }
                break;
            case PassiveItemSlotName.Arm:
                if (!isArmEquipped)
                {
                    armPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (armPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.OminousGripOfThunder)
                    {
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.05f, 2);
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.1f, 2);
                    }
                    else if (armPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EmbercladBracers)
                    {
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.1f, 2);
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.08f, 2);
                        player.additionalBowAttackCoolDownModifier -= 0.5f;
                        player.additionalMeleeAttackCoolDownModifier -= 0.5f;
                    }
                    else if (armPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.VenomTouchedGloves)
                    {
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue + 0.1f, 2);
                        player.isImmunetoPoison = true;
                    }

                    isArmEquipped = true;
                }
                else
                {
                    // Arm slot is full, equip in inventory slot if there is room
                    if (!InventoryManager.Instance.IsInventoryFull() && !setPassiveItemEventArgs.isSwap)
                    {
                        setPassiveItemEventArgs.equipResult.placedIntoInventory = true;
                    }
                }
                break;
            case PassiveItemSlotName.Leg:
                if (!isLegEquipped)
                {
                    legPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (legPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WingedSandals)
                    {
                        player.CurrentAgilityValue += 2;
                    }
                    else if (legPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BootsOfInfernalMarch)
                    {
                        player.CurrentAgilityValue++;
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.15f, 2);
                    }

                    isLegEquipped = true;
                }
                else
                {
                    // Leg slot is full, equip in inventory slot if there is room
                    if (!InventoryManager.Instance.IsInventoryFull() && !setPassiveItemEventArgs.isSwap)
                    {
                        setPassiveItemEventArgs.equipResult.placedIntoInventory = true;
                    }
                }
                break;
            default:
                break;
        }

        // Update stats values after weapon switch
        player.RecalculateSecondaryStats();

        // Update stat value displays on book ui
        StaticEventHandler.CallPrimaryStatsChangedEvent();
    }

    private void SetPassiveItemEvent_OnRemovedPassiveItem(SetPassiveItemEvent setPassiveItemEvent, SetPassiveItemEventArgs setPassiveItemEventArgs)
    {
        player = GameManager.Instance.GetPlayer();

        // Place item to inventory slot if inventory is not full and it is really drag into inventory action - NOT DROP
        if (!InventoryManager.Instance.IsInventoryFull() && setPassiveItemEventArgs.equipResult != null && !setPassiveItemEventArgs.isSwap)
        {
            setPassiveItemEventArgs.equipResult.placedIntoInventory = true;
        }

        switch (setPassiveItemEventArgs.passiveItemSlotName)
        {
            case PassiveItemSlotName.Head:
                if (isHeadEquipped)
                {
                    if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HaloOfBlindingRadiance)
                    {
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue - 0.1f, 2);
                        player.additionalBlindMakerModifier -= 0.1f;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WardenOfForest)
                    {
                        player.additionalBowAccuracyModifier -= 0.3f;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HelmOfTheEternalVigil)
                    {
                        player.CurrentDexterityValue--;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.1f, 2);
                        player.additionalEvasivenessModifier -= 0.1f;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EnchantersSpire)
                    {
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.05f, 2);
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue - 0.05f, 2);
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.05f, 2);
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue - 0.05f, 2);
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue - 0.05f, 2);
                        player.currentDarkResistanceValue = (float)Math.Round(player.currentDarkResistanceValue - 0.05f, 2);
                        player.CurrentIntelligenceValue -= 1;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WhisperingHood)
                    {
                        player.CurrentDexterityValue--;
                        player.additionalEvasivenessModifier -= 0.15f;
                    }
                    else if (headPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GildedGuardian)
                    {
                        player.CurrentConstitutionValue--;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.2f, 2);
                    }

                    headPassiveItem = null;
                    isHeadEquipped = false;
                }
                break;
            case PassiveItemSlotName.Chest:
                if (isChestEquipped)
                {
                    if (chestPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ChestplateOfTheLastLight)
                    {
                        player.CurrentStrengthValue--;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.3f, 2);
                    }
                    else if (chestPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BlazingHeartplate)
                    {
                        player.CurrentStrengthValue--;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.2f, 2);
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.1f, 2);
                        player.additionalBowAttackCoolDownModifier += 0.05f;
                        player.additionalMeleeAttackCoolDownModifier += 0.05f;
                    }
                    else if (chestPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.FrostboundChainmail)
                    {
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.15f, 2);
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue - 0.1f, 2);
                        player.isImmunetoFrost = false;
                    }
                    else if (chestPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.VenomweaveVest)
                    {
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.1f, 2);
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue - 0.1f, 2);
                        player.isImmunetoPoison = false;
                    }

                    chestPassiveItem = null;
                    isChestEquipped = false;
                }
                break;
            case PassiveItemSlotName.Neck:
                if (isNeckEquipped)
                {
                    if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RubyPendant)
                    {
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.2f, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EmeraldPendant)
                    {
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue - 0.2f, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.TopazPendant)
                    {
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.2f, 2);
                    }
                    else if (neckPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SapphirePendant)
                    {
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue - 0.2f, 2);
                    }

                    neckPassiveItem = null;
                    isNeckEquipped = false;
                }
                break;
            case PassiveItemSlotName.Finger:
                if (isFingerEquipped)
                {
                    if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfTempestStrikes)
                    {
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.1f, 2);
                        player.additionalBowAttackCoolDownModifier += 0.2f;
                        player.additionalMeleeAttackCoolDownModifier += 0.2f;
                    }
                    else if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfMight)
                    {
                        player.CurrentStrengthValue--;
                    }
                    else if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfVitality)
                    {
                        player.CurrentConstitutionValue--;
                    }
                    else if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfSagacity)
                    {
                        player.CurrentIntelligenceValue--;
                    }
                    else if (fingerPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfFortune)
                    {
                        player.additionalDropChanceModifier -= 0.15f;
                    }

                    fingerPassiveItem = null;
                    isFingerEquipped = false;
                }
                break;
            case PassiveItemSlotName.Back:
                if (isBackEquipped)
                {
                    if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                    {
                        player.additionalMeleeCriticalHitChanceModifier = (float)Math.Round(player.additionalMeleeCriticalHitChanceModifier - 0.05f, 2);
                        player.shadowCloakEquipped = false; // Put here at the end intentionally, because additional cr. should be nullified above first
                    }
                    else if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RecantersCloak)
                    {
                        player.CurrentAgilityValue--;
                        player.additionalEvasivenessModifier -= 0.1f;
                    }
                    else if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.MantleOfStars)
                    {
                        player.additionalElementalDamageModifier -= 0.05f;
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.15f, 2);
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue - 0.15f, 2);
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.15f, 2);
                        player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue - 0.15f, 2);
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue - 0.15f, 2);
                        player.currentDarkResistanceValue = (float)Math.Round(player.currentDarkResistanceValue - 0.15f, 2);
                    }
                    else if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.CloakOfWindwalker)
                    {
                        player.CurrentAgilityValue -= 2;
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.3f, 2);
                    }
                    else if (backPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GoldenCloak)
                    {
                        player.CurrentStrengthValue--;
                        player.CurrentDexterityValue--;
                        player.CurrentConstitutionValue--;
                        player.CurrentIntelligenceValue--;
                        player.CurrentAgilityValue--;
                    }

                    backPassiveItem = null;
                    isBackEquipped = false;
                }
                break;
            case PassiveItemSlotName.Waist:
                if (isWaistEquipped)
                {
                    if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BeltOfSorcery)
                    {
                        player.CurrentIntelligenceValue--;
                        player.additionalCastDurationModifier += 0.2f;
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.InfernoSash)
                    {
                        player.CurrentConstitutionValue--;
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.05f, 2);
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.15f, 2);
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GirdleOfFirmament)
                    {
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.1f, 2);
                        player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue - 0.1f, 2);
                        player.isImmunetoBlind = false;
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BloodforgedGirdle)
                    {
                        player.CurrentStrengthValue--;
                        player.CurrentAgilityValue--;
                        player.additionalMeleeAttackCoolDownModifier += 0.05f;
                    }
                    else if (waistPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SandweaversSash)
                    {
                        player.CurrentDexterityValue--;
                        player.currentEvasivenessValue -= 0.1f;
                        player.additionalMeleeCriticalHitChanceModifier -= 0.05f;
                    }

                    waistPassiveItem = null;
                    isWaistEquipped = false;
                }
                break;
            case PassiveItemSlotName.Arm:
                if (isArmEquipped)
                {
                    if (armPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.OminousGripOfThunder)
                    {
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.05f, 2);
                        player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.1f, 2);
                    }
                    else if (armPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EmbercladBracers)
                    {
                        player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.1f, 2);
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.08f, 2);
                        player.additionalBowAttackCoolDownModifier += 0.5f;
                        player.additionalMeleeAttackCoolDownModifier += 0.5f;
                    }
                    else if (armPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.VenomTouchedGloves)
                    {
                        player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue - 0.1f, 2);
                        player.isImmunetoPoison = false;
                    }
                    armPassiveItem = null;
                    isArmEquipped = false;
                }
                break;
            case PassiveItemSlotName.Leg:
                if (isLegEquipped)
                {
                    if (legPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WingedSandals)
                    {
                        player.CurrentAgilityValue -= 2;
                    }
                    else if (legPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BootsOfInfernalMarch)
                    {
                        player.CurrentAgilityValue--;
                        player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.15f, 2);
                    }

                    legPassiveItem = null;
                    isLegEquipped = false;
                }
                break;
            default:
                break;
        }

        // Update stats values after weapon switch
        player.RecalculateSecondaryStats();

        // Update stat value displays on book ui
        StaticEventHandler.CallPrimaryStatsChangedEvent();
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
