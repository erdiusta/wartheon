using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpecialMoveUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion Header OBJECT REFERENCES
    #region Tooltip
    [Tooltip("Populate with the Passive Skill object")]
    #endregion Tooltip
    [SerializeField] Transform passiveSkillContainer;
    #region Tooltip
    [Tooltip("Populate with the Active Skill One object")]
    #endregion Tooltip
    [SerializeField] Transform activeSkillOneContainer;
    #region Tooltip
    [Tooltip("Populate with the Active Skill Two object")]
    #endregion Tooltip
    [SerializeField] Transform activeSkillTwoContainer;
    #region Tooltip
    [Tooltip("Populate with the Active Skill Three object")]
    #endregion Tooltip
    [SerializeField] Transform activeSkillThreeContainer;

    Transform[] activeSkillSlotContainers;

    [Space(10)]
    [SerializeField] Sprite noSkillSprite;
    [Space(10)]

    Player player;
    Coroutine specialMoveOneCooldownCoroutine;
    Coroutine specialMoveTwoCooldownCoroutine;
    Coroutine specialMoveThreeCooldownCoroutine;

    bool[] specialMoveResetArray = new bool[3];

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        player.specialMoveEvent.OnSpecialMoveUsed += SpecialMoveEvent_OnSpecialMoveUsed;

        StaticEventHandler.OnActiveUniqueSkillPlaced += StaticEventHandler_OnActiveUniqueSkillPlaced;
    }

    private void OnDisable()
    {
        player.specialMoveEvent.OnSpecialMoveUsed -= SpecialMoveEvent_OnSpecialMoveUsed;

        StaticEventHandler.OnActiveUniqueSkillPlaced += StaticEventHandler_OnActiveUniqueSkillPlaced;
    }

    private void Start()
    {
        // Populate special move imagaes based on selected character
        passiveSkillContainer.GetChild(0).GetComponent<Image>().sprite = player.playerDetails.passiveSkillImage;
        activeSkillOneContainer.GetChild(0).GetComponent<Image>().sprite = noSkillSprite;
        activeSkillTwoContainer.GetChild(0).GetComponent<Image>().sprite = noSkillSprite;
        activeSkillThreeContainer.GetChild(0).GetComponent<Image>().sprite = noSkillSprite;

        activeSkillSlotContainers = new Transform[3] { activeSkillOneContainer, activeSkillTwoContainer, activeSkillThreeContainer };
    }

    private void StaticEventHandler_OnActiveUniqueSkillPlaced(ActiveUniqueSkillPlacedArgs activeUniqueSkillPlacedArgs)
    {
        activeSkillSlotContainers[activeUniqueSkillPlacedArgs.placedSlotIndex - 1].GetChild(0).GetComponent<Image>().sprite =
            activeUniqueSkillPlacedArgs.activeUniqueSkillDetails.activeUniqueSkillSprite;
    }

    private void Update()
    {
        SlotSkillUpdate(1);
        SlotSkillUpdate(2);
        SlotSkillUpdate(3);
    }

    private void SlotSkillUpdate(int slotIndex)
    {
        if (player.specialMovesCooldownCheckArray[slotIndex - 1])
        {
            specialMoveResetArray[slotIndex - 1] = false;

            player.specialMoveCooldownTimerArray[slotIndex - 1] += Time.deltaTime;

            float duration = player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
                * (1 + player.buffDurationModifier);

            if (duration > 0) player.specialMoveDurationTimerArray[slotIndex - 1] += Time.deltaTime;

            float cooldown = player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillCooldownDuration
                * (1 + player.cooldownDurationModifier);

            if (player.specialMoveCooldownTimerArray[slotIndex - 1] > cooldown)
            {
                player.specialMovesCooldownCheckArray[slotIndex - 1] = false;
                player.specialMoveCooldownTimerArray[slotIndex - 1] = 0f;
                player.specialMoveDurationTimerArray[slotIndex - 1] = 0f;
                ResetSpecialMoveCooldownSlot(slotIndex);
            }
        }
    }

    private void SpecialMoveEvent_OnSpecialMoveUsed(SpecialMoveEvent specialMoveEvent, SpecialMoveEventArgs specialMoveEventArgs)
    {
        if (specialMoveEventArgs.onlyChangeAlpha)
        {
            Image specialMoveCooldownImage = activeSkillOneContainer.GetChild(1).GetComponent<Image>();
            Image specialMoveImage = activeSkillOneContainer.GetChild(0).GetComponent<Image>();

            // update cooldownCircle
            specialMoveCooldownImage.fillAmount = 1;
        }
        else
        {
            switch (specialMoveEventArgs.specialMoveNumber)
            {
                case 1:
                    StopSpecialMoveCoroutine(specialMoveOneCooldownCoroutine);
                    specialMoveOneCooldownCoroutine = StartCoroutine(UpdateCooldownSlotRoutine(1));
                    break;
                case 2:
                    StopSpecialMoveCoroutine(specialMoveTwoCooldownCoroutine);
                    specialMoveTwoCooldownCoroutine = StartCoroutine(UpdateCooldownSlotRoutine(2));
                    break;
                case 3:
                    StopSpecialMoveCoroutine(specialMoveThreeCooldownCoroutine);
                    specialMoveThreeCooldownCoroutine = StartCoroutine(UpdateCooldownSlotRoutine(3));
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Stop coroutine updating special move progress bar
    /// </summary>
    private void StopSpecialMoveCoroutine(Coroutine coroutine)
    {
        // Stop any active weapon reload bar on the UI
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    /// <summary>
    /// Animate special cooldown slot coroutine
    /// </summary>
    private IEnumerator UpdateCooldownSlotRoutine(int specialMoveNumber)
    {
        Image specialMoveCooldownImage;
        Image specialMoveImage;

        // Animate the weapon reload bar
        while (player.specialMoveCooldownTimerArray[specialMoveNumber - 1] < player.currentlyUsedActiveUniqueSkills[specialMoveNumber].activeUniqueSkillCooldownDuration
                * (1 + player.cooldownDurationModifier))
        {
            if (!specialMoveResetArray[specialMoveNumber - 1])
            {
                specialMoveCooldownImage = activeSkillSlotContainers[specialMoveNumber - 1].GetChild(1).GetComponent<Image>();
                specialMoveImage = activeSkillSlotContainers[specialMoveNumber - 1].GetChild(0).GetComponent<Image>();
                specialMoveCooldownImage.gameObject.SetActive(true);

                // update cooldownCircle
                float circleFill = Mathf.Clamp(player.specialMoveCooldownTimerArray[specialMoveNumber - 1] / 
                    player.currentlyUsedActiveUniqueSkills[specialMoveNumber].activeUniqueSkillCooldownDuration, 0, 1);
                specialMoveCooldownImage.fillAmount = 1 - circleFill;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Reset special move bar coroutine
    private void ResetSpecialMoveCooldownSlot(int specialMoveNum)
    {
        Image specialMoveCooldownImage;
        Image specialMoveImage;

        specialMoveCooldownImage = activeSkillSlotContainers[specialMoveNum - 1].GetChild(1).GetComponent<Image>();
        specialMoveImage = activeSkillSlotContainers[specialMoveNum - 1].GetChild(0).GetComponent<Image>();
        specialMoveCooldownImage.fillAmount = 1;
        specialMoveCooldownImage.gameObject.SetActive(false);
        specialMoveResetArray[specialMoveNum - 1] = true;
    }
}
