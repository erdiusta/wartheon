using UnityEngine;
using System;
using static UnityEngine.Rendering.GPUSort;

public class SelectedPassiveItem : MonoBehaviour
{
    Player player;
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

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void SetPassiveItemEvent_OnEquippedPassiveItem(SetPassiveItemEvent _, SetPassiveItemEventArgs args)
    {
        TryEquipPassiveItem(args.passiveItem, args.isSwap);

        // Update stats values after weapon switch
        player.RecalculateSecondaryStats();

        // Update stat value displays on book ui
        StaticEventHandler.CallPrimaryStatsChangedEvent();
    }

    private void SetPassiveItemEvent_OnRemovedPassiveItem(SetPassiveItemEvent _, SetPassiveItemEventArgs args)
    {
        if (player.equippedPassiveItems.TryGetValue(args.passiveItemSlotName, out PassiveItem equippedItem) && equippedItem != null)
        {
            RemovePassiveEffects(equippedItem);
            player.equippedPassiveItems[args.passiveItemSlotName] = null; // Item removed from passive slot
        }

        if (!InventoryManager.Instance.IsInventoryFull() && !args.isSwap && !args.dropButton)
        {
            args.passiveItem.itemSlotStatus = ItemSlotStatus.Inventory;
            InventoryManager.Instance.PlaceItemToLowestPossibleIndexSlot(args.passiveItem); // Item added to inventory slot
        }

        player.RecalculateSecondaryStats();
        StaticEventHandler.CallPrimaryStatsChangedEvent();
    }

    private void TryEquipPassiveItem(PassiveItem newItem, bool isSwap)
    {
        PassiveItemSlotName slot = newItem.passiveItemDetails.passiveItemSlotName;

        // If already equipped and not a swap, move to inventory
        if (player.equippedPassiveItems[slot] != null && !isSwap)
        {
            // Swap: move currently equipped item to inventory
            if (!InventoryManager.Instance.IsInventoryFull())
            {
                // Place current item to inventory
                InventoryManager.Instance.PlaceItemToLowestPossibleIndexSlot(player.equippedPassiveItems[slot]);
            }

            // Remove old item effects
            RemovePassiveEffects(player.equippedPassiveItems[slot]);
        }

        // Equip new item and apply effects
        player.equippedPassiveItems[slot] = newItem;
        ApplyPassiveEffects(newItem);
    }

    public PassiveItem GetCurrentPassiveItem(PassiveItemSlotName slot)
    {
        player.equippedPassiveItems.TryGetValue(slot, out PassiveItem item);
        return item;
    }

    private void ApplyPassiveEffects(PassiveItem item)
    {
        switch (item.passiveItemDetails.passiveItemSlotName)
        {
            case PassiveItemSlotName.Head:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.WardenOfForest)
                {
                    player.additionalBowAccuracyModifier += 0.3f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.HaloOfBlindingRadiance)
                {
                    player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue + 0.1f, 2);
                    player.additionalBlindMakerModifier += 0.1f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.HelmOfTheEternalVigil)
                {
                    player.CurrentDexterityValue++;
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.1f, 2);
                    player.additionalEvasivenessModifier += 0.1f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.EnchantersSpire)
                {
                    player.CurrentIntelligenceValue++;
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.05f, 2);
                    player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue + 0.05f, 2);
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.05f, 2);
                    player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue + 0.05f, 2);
                    player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue + 0.05f, 2);
                    player.currentDarkResistanceValue = (float)Math.Round(player.currentDarkResistanceValue + 0.05f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.WhisperingHood)
                {
                    player.CurrentDexterityValue++;
                    player.additionalEvasivenessModifier += 0.15f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.GildedGuardian)
                {
                    player.CurrentConstitutionValue++;
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.2f, 2);
                }
                break;
            case PassiveItemSlotName.Chest:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.ChestplateOfTheLastLight)
                {
                    player.CurrentStrengthValue++;
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.3f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.BlazingHeartplate)
                {
                    player.CurrentStrengthValue++;
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.2f, 2);
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.1f, 2);
                    player.additionalBowAttackCoolDownModifier -= 0.05f;
                    player.additionalMeleeAttackCoolDownModifier -= 0.05f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.FrostboundChainmail)
                {
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.15f, 2);
                    player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue + 0.1f, 2);
                    player.isImmunetoFrost = true;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.VenomweaveVest)
                {
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.1f, 2);
                    player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue + 0.1f, 2);
                    player.isImmunetoPoison = true;
                }
                break;
            case PassiveItemSlotName.Neck:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.RubyPendant)
                {
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.2f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.EmeraldPendant)
                {
                    player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue + 0.2f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.TopazPendant)
                {
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.2f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.SapphirePendant)
                {
                    player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue + 0.2f, 2);
                }
                break;
            case PassiveItemSlotName.Finger:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.RingOfTempestStrikes)
                {
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.1f, 2);
                    player.additionalBowAttackCoolDownModifier -= 0.2f;
                    player.additionalMeleeAttackCoolDownModifier -= 0.2f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.RingOfMight)
                {
                    player.CurrentStrengthValue++;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.RingOfVitality)
                {
                    player.CurrentConstitutionValue++;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.RingOfSagacity)
                {
                    player.CurrentIntelligenceValue++;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.RingOfFortune)
                {
                    player.additionalDropChanceModifier += 0.15f;
                }
                break;
            case PassiveItemSlotName.Back:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                {
                    player.additionalMeleeCriticalHitChanceModifier = (float)Math.Round(player.additionalMeleeCriticalHitChanceModifier + 0.05f, 2);
                    player.shadowCloakEquipped = true;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.RecantersCloak)
                {
                    player.CurrentAgilityValue++;
                    player.additionalEvasivenessModifier += 0.1f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.MantleOfStars)
                {
                    player.additionalElementalDamageModifier += 0.05f;
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.15f, 2);
                    player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue + 0.15f, 2);
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.15f, 2);
                    player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue + 0.15f, 2);
                    player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue + 0.15f, 2);
                    player.currentDarkResistanceValue = (float)Math.Round(player.currentDarkResistanceValue + 0.15f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.CloakOfWindwalker)
                {
                    player.CurrentAgilityValue += 2;
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.3f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.GoldenCloak)
                {
                    player.CurrentStrengthValue++;
                    player.CurrentDexterityValue++;
                    player.CurrentConstitutionValue++;
                    player.CurrentIntelligenceValue++;
                    player.CurrentAgilityValue++;
                }
                break;
            case PassiveItemSlotName.Waist:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.BeltOfSorcery)
                {
                    player.CurrentIntelligenceValue++;
                    player.additionalCastDurationModifier -= 0.2f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.InfernoSash)
                {
                    player.CurrentConstitutionValue++;
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.05f, 2);
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.15f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.GirdleOfFirmament)
                {
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.1f, 2);
                    player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue + 0.1f, 2);
                    player.isImmunetoBlind = true;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.BloodforgedGirdle)
                {
                    player.CurrentStrengthValue++;
                    player.CurrentAgilityValue++;
                    player.additionalMeleeAttackCoolDownModifier -= 0.05f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.SandweaversSash)
                {
                    player.CurrentDexterityValue++;
                    player.additionalEvasivenessModifier += 0.1f;
                    player.additionalMeleeCriticalHitChanceModifier += 0.05f;
                }
                break;
            case PassiveItemSlotName.Arm:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.OminousGripOfThunder)
                {
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.05f, 2);
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue + 0.1f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.EmbercladBracers)
                {
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.1f, 2);
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.08f, 2);
                    player.additionalBowAttackCoolDownModifier -= 0.05f;
                    player.additionalMeleeAttackCoolDownModifier -= 0.05f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.VenomTouchedGloves)
                {
                    player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue + 0.1f, 2);
                    player.isImmunetoPoison = true;
                }
                break;
            case PassiveItemSlotName.Leg:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.WingedSandals)
                {
                    player.CurrentAgilityValue += 2;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.BootsOfInfernalMarch)
                {
                    player.CurrentAgilityValue++;
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue + 0.15f, 2);
                }
                break;
            default:
                break;
        }
    }

    private void RemovePassiveEffects(PassiveItem item)
    {
        switch (item.passiveItemDetails.passiveItemSlotName)
        {
            case PassiveItemSlotName.Head:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.HaloOfBlindingRadiance)
                {
                    player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue - 0.1f, 2);
                    player.additionalBlindMakerModifier -= 0.1f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.WardenOfForest)
                {
                    player.additionalBowAccuracyModifier -= 0.3f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.HelmOfTheEternalVigil)
                {
                    player.CurrentDexterityValue--;
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.1f, 2);
                    player.additionalEvasivenessModifier -= 0.1f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.EnchantersSpire)
                {
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.05f, 2);
                    player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue - 0.05f, 2);
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.05f, 2);
                    player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue - 0.05f, 2);
                    player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue - 0.05f, 2);
                    player.currentDarkResistanceValue = (float)Math.Round(player.currentDarkResistanceValue - 0.05f, 2);
                    player.CurrentIntelligenceValue -= 1;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.WhisperingHood)
                {
                    player.CurrentDexterityValue--;
                    player.additionalEvasivenessModifier -= 0.15f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.GildedGuardian)
                {
                    player.CurrentConstitutionValue--;
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.2f, 2);
                }
                break;
            case PassiveItemSlotName.Chest:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.ChestplateOfTheLastLight)
                {
                    player.CurrentStrengthValue--;
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.3f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.BlazingHeartplate)
                {
                    player.CurrentStrengthValue--;
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.2f, 2);
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.1f, 2);
                    player.additionalBowAttackCoolDownModifier += 0.05f;
                    player.additionalMeleeAttackCoolDownModifier += 0.05f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.FrostboundChainmail)
                {
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.15f, 2);
                    player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue - 0.1f, 2);
                    player.isImmunetoFrost = false;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.VenomweaveVest)
                {
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.1f, 2);
                    player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue - 0.1f, 2);
                    player.isImmunetoPoison = false;
                }
                break;
            case PassiveItemSlotName.Neck:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.RubyPendant)
                {
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.2f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.EmeraldPendant)
                {
                    player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue - 0.2f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.TopazPendant)
                {
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.2f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.SapphirePendant)
                {
                    player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue - 0.2f, 2);
                }
                break;
            case PassiveItemSlotName.Finger:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.RingOfTempestStrikes)
                {
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue + 0.1f, 2);
                    player.additionalBowAttackCoolDownModifier += 0.2f;
                    player.additionalMeleeAttackCoolDownModifier += 0.2f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.RingOfMight)
                {
                    player.CurrentStrengthValue--;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.RingOfVitality)
                {
                    player.CurrentConstitutionValue--;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.RingOfSagacity)
                {
                    player.CurrentIntelligenceValue--;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.RingOfFortune)
                {
                    player.additionalDropChanceModifier -= 0.15f;
                }
                break;
            case PassiveItemSlotName.Back:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                {
                    player.additionalMeleeCriticalHitChanceModifier = (float)Math.Round(player.additionalMeleeCriticalHitChanceModifier - 0.05f, 2);
                    player.shadowCloakEquipped = false; // Put here at the end intentionally, because additional cr. should be nullified above first
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.RecantersCloak)
                {
                    player.CurrentAgilityValue--;
                    player.additionalEvasivenessModifier -= 0.1f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.MantleOfStars)
                {
                    player.additionalElementalDamageModifier -= 0.05f;
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.15f, 2);
                    player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue - 0.15f, 2);
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.15f, 2);
                    player.currentWaterResistanceValue = (float)Math.Round(player.currentWaterResistanceValue - 0.15f, 2);
                    player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue - 0.15f, 2);
                    player.currentDarkResistanceValue = (float)Math.Round(player.currentDarkResistanceValue - 0.15f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.CloakOfWindwalker)
                {
                    player.CurrentAgilityValue -= 2;
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.3f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.GoldenCloak)
                {
                    player.CurrentStrengthValue--;
                    player.CurrentDexterityValue--;
                    player.CurrentConstitutionValue--;
                    player.CurrentIntelligenceValue--;
                    player.CurrentAgilityValue--;
                }
                break;
            case PassiveItemSlotName.Waist:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.BeltOfSorcery)
                {
                    player.CurrentIntelligenceValue--;
                    player.additionalCastDurationModifier += 0.2f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.InfernoSash)
                {
                    player.CurrentConstitutionValue--;
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.05f, 2);
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.15f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.GirdleOfFirmament)
                {
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.1f, 2);
                    player.currentLightResistanceValue = (float)Math.Round(player.currentLightResistanceValue - 0.1f, 2);
                    player.isImmunetoBlind = false;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.BloodforgedGirdle)
                {
                    player.CurrentStrengthValue--;
                    player.CurrentAgilityValue--;
                    player.additionalMeleeAttackCoolDownModifier += 0.05f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.SandweaversSash)
                {
                    player.CurrentDexterityValue--;
                    player.currentEvasivenessValue -= 0.1f;
                    player.additionalMeleeCriticalHitChanceModifier -= 0.05f;
                }
                break;
            case PassiveItemSlotName.Arm:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.OminousGripOfThunder)
                {
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.05f, 2);
                    player.currentAirResistanceValue = (float)Math.Round(player.currentAirResistanceValue - 0.1f, 2);
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.EmbercladBracers)
                {
                    player.currentPhysicalResistanceValue = (float)Math.Round(player.currentPhysicalResistanceValue - 0.1f, 2);
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.08f, 2);
                    player.additionalBowAttackCoolDownModifier += 0.05f;
                    player.additionalMeleeAttackCoolDownModifier += 0.05f;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.VenomTouchedGloves)
                {
                    player.currentEarthResistanceValue = (float)Math.Round(player.currentEarthResistanceValue - 0.1f, 2);
                    player.isImmunetoPoison = false;
                }
                break;
            case PassiveItemSlotName.Leg:
                if (item.passiveItemDetails.passiveItemType == PassiveItemType.WingedSandals)
                {
                    player.CurrentAgilityValue -= 2;
                }
                else if (item.passiveItemDetails.passiveItemType == PassiveItemType.BootsOfInfernalMarch)
                {
                    player.CurrentAgilityValue--;
                    player.currentFireResistanceValue = (float)Math.Round(player.currentFireResistanceValue - 0.15f, 2);
                }
                break;
            default:
                break;
        }
    }
}
