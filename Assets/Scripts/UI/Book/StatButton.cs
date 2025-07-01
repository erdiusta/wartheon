using UnityEngine;

public class StatButton : MonoBehaviour
{
    public PrimaryStatName statName;

    Player player;

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();
    }

    public void IncreaseStatPoint()
    {
        if(player.currentSkillPoints > 0)
        {
            switch (statName)
            {
                case PrimaryStatName.Strength:
                    player.CurrentStrengthValue++;
                    break;
                case PrimaryStatName.Constitution:
                    player.CurrentConstitutionValue++;
                    break;
                case PrimaryStatName.Dexterity:
                    player.CurrentDexterityValue++;
                    break;
                case PrimaryStatName.Intelligence:
                    player.CurrentIntelligenceValue++;
                    break;
                case PrimaryStatName.Willpower:
                    player.CurrentWillpowerValue++;
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
            StaticEventHandler.CallStatPointChangedEvent(statName);
        }
    }
}
