using UnityEngine;

public class StatButton : MonoBehaviour
{
    public PrimaryStatName statName;

    Player player;

    private void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();
    }

    public void IncreaseStatPoint()
    {
        if(player.currentStatPoints > 0)
        {
            switch (statName)
            {
                case PrimaryStatName.Strength:
                    player.CurrentStrengthValue++;
                    break;
                case PrimaryStatName.Constitution:
                    player.CurrentConstitutionValue++;
                    player.healthEvent.CallHealthChangedEvent(player.health.currentHealth, 0, MeleeHand.None);
                    break;
                case PrimaryStatName.Dexterity:
                    player.CurrentDexterityValue++;
                    break;
                case PrimaryStatName.Intelligence:
                    player.CurrentIntelligenceValue++;
                    break;
                case PrimaryStatName.Willpower:
                    player.CurrentWillpowerValue++;
                    player.manaEvent.CallManaChangedEvent(player.mana.currentMana, false);
                    break;
                case PrimaryStatName.Agility:
                    player.CurrentAgilityValue++;
                    break;
                case PrimaryStatName.Resolve:
                    player.CurrentResolveValue++;
                    break;
                case PrimaryStatName.Ferocity:
                    player.CurrentFerocityValue++;
                    break;
                default:
                    break;
            }

            player.currentStatPoints--;
            StaticEventHandler.CallStatPointChangedEvent();
        }
    }
}
