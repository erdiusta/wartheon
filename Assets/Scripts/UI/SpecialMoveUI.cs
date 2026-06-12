using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpecialMoveUI : MonoBehaviour
{
    Transform passiveSkillContainer;
    Transform activeSkillOneContainer;
    Transform activeSkillTwoContainer;
    Transform activeSkillThreeContainer;

    Transform[] activeSkillSlotContainers;

    [Space(10)]
    [SerializeField] Sprite noSkillSprite;
    [Space(10)]

    Coroutine specialMoveOneCooldownCoroutine;
    Coroutine specialMoveTwoCooldownCoroutine;
    Coroutine specialMoveThreeCooldownCoroutine;

    Coroutine specialMoveOneRecastCoroutine;
    Coroutine specialMoveTwoRecastCoroutine;
    Coroutine specialMoveThreeRecastCoroutine;

    Player player;
    bool[] specialMoveResetArray = new bool[3];

    private void Awake()
    {
        passiveSkillContainer = transform.GetChild(0);
        activeSkillOneContainer = transform.GetChild(1);
        activeSkillTwoContainer = transform.GetChild(2);
        activeSkillThreeContainer = transform.GetChild(3);

        activeSkillSlotContainers = new Transform[3] { activeSkillOneContainer, activeSkillTwoContainer, activeSkillThreeContainer };
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForPlayerInitialization());
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    IEnumerator WaitForPlayerInitialization()
    {
        while (player == null || player?.specialMoveEvent == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetLocalPlayer();
            yield return null;
        }

        Subscribe();
        PopulateAllSkillIconsOnInit();
    }

    private void Subscribe()
    {
        player.specialMoveEvent.OnSpecialMoveUsed += SpecialMoveEvent_OnSpecialMoveUsed;
        player.specialMoveEvent.OnSpecialMoveCooldownReset += SpecialMoveEvent_OnSpecialMoveCooldownReset;
        StaticEventHandler.OnActiveUniqueSkillPlaced += StaticEventHandler_OnActiveUniqueSkillPlaced;
    }

    private void Unsubscribe()
    {
        player.specialMoveEvent.OnSpecialMoveUsed -= SpecialMoveEvent_OnSpecialMoveUsed;
        player.specialMoveEvent.OnSpecialMoveCooldownReset -= SpecialMoveEvent_OnSpecialMoveCooldownReset;
        StaticEventHandler.OnActiveUniqueSkillPlaced -= StaticEventHandler_OnActiveUniqueSkillPlaced;
    }

    private void PopulateAllSkillIconsOnInit()
    {
        // Call event in order to update SkillUI in Gameplay HUD
        for (int i = 1; i <= 3; i++)
        {
            // Populate first three skill
            player.currentlyUsedActiveUniqueSkills.Add(i, player.playersAllActiveUniqueSkills[i - 1]);
        }

        passiveSkillContainer.GetChild(0).GetComponent<Image>().sprite = player.playerDetails.passiveSkillImage;

        for (int i = 1; i <= 3; i++)
        {
            if(player.currentlyUsedActiveUniqueSkills.TryGetValue(i, out var skill))
            {
                activeSkillSlotContainers[i - 1].GetChild(0).GetComponent<Image>().sprite = player.currentlyUsedActiveUniqueSkills[i].activeUniqueSkillSprite;
            }
        }
    }

    private void StaticEventHandler_OnActiveUniqueSkillPlaced(ActiveUniqueSkillPlacedArgs args)
    {
        StartCoroutine(SkillIconPlacementRoutine(args));
    }

    private void SpecialMoveEvent_OnSpecialMoveCooldownReset(SpecialMoveEvent arg1, SpecialMoveEventArgs arg2) { }

    IEnumerator SkillIconPlacementRoutine(ActiveUniqueSkillPlacedArgs activeUniqueSkillPlacedArgs)
    {
        while (player == null || player.playersAllActiveUniqueSkills == null || player.playersAllActiveUniqueSkills.Length < 3) yield return null;

        yield return new WaitForSeconds(0.25f);

        if (activeUniqueSkillPlacedArgs.slotDrop)
        {
            if (activeSkillSlotContainers != null && activeSkillSlotContainers.Length > activeUniqueSkillPlacedArgs.placedSlotIndex - 1)
            {
                activeSkillSlotContainers[activeUniqueSkillPlacedArgs.placedSlotIndex - 1].GetChild(0).GetComponent<Image>().sprite =
                    activeUniqueSkillPlacedArgs.activeUniqueSkillDetails.activeUniqueSkillSprite;
            }
        }
        else
        {
            passiveSkillContainer = transform.GetChild(0);
            activeSkillOneContainer = transform.GetChild(1);
            activeSkillTwoContainer = transform.GetChild(2);
            activeSkillThreeContainer = transform.GetChild(3);

            activeSkillSlotContainers = new Transform[3]
            {
                activeSkillOneContainer, activeSkillTwoContainer, activeSkillThreeContainer
            };

            passiveSkillContainer.GetChild(0).GetComponent<Image>().sprite = player.playerDetails.passiveSkillImage;
            activeSkillOneContainer.GetChild(0).GetComponent<Image>().sprite = player.playersAllActiveUniqueSkills[0].activeUniqueSkillSprite;
            activeSkillTwoContainer.GetChild(0).GetComponent<Image>().sprite = player.playersAllActiveUniqueSkills[1].activeUniqueSkillSprite;
            activeSkillThreeContainer.GetChild(0).GetComponent<Image>().sprite = player.playersAllActiveUniqueSkills[2].activeUniqueSkillSprite;
        }
    }

    private void Update()
    {
        if (player == null) return;

        if (player.currentlyUsedActiveUniqueSkills != null && player.currentlyUsedActiveUniqueSkills.Count > 0)
        {
            SlotSkillUpdate(1);
            SlotSkillUpdate(2);
            SlotSkillUpdate(3);
        }
    }

    private void SlotSkillUpdate(int slotIndex)
    {
        int idx = slotIndex - 1;

        ActiveUniqueSkillDetailsSO usedActiveUniqueSkillContainer = player.currentlyUsedActiveUniqueSkills[slotIndex];
        int activeSkillLevel = usedActiveUniqueSkillContainer.GetCurrentActiveLevel();
        var activeSkillData = usedActiveUniqueSkillContainer.levels[activeSkillLevel - 1];

        if (player.specialMoveRecastCountArray[idx] > 0)
        {
            specialMoveResetArray[idx] = false;

            player.specialMoveRecastCooldownTimerArray[idx] += Time.deltaTime;

            float recastDuration = activeSkillData.recastWindowDuration * (1 + player.currentSkillDurationModifier);

            if (recastDuration > 0) player.specialMoveRecastDurationTimerArray[idx] += Time.deltaTime;

            float recastCooldown = activeSkillData.recastCooldown * (1 - player.currentSkillCooldownReducer);

            // If recast is not triggered during recast duration time, reset cooldown
            if (player.specialMoveRecastCooldownTimerArray[idx] > recastCooldown && player.specialMoveRecastCountArray[idx] > 0)
            {
                ReduceRecastRepeatCount(slotIndex, inUpdate: true);
            }
        }
        else if (player.specialMovesCooldownCheckArray[idx])
        {
            specialMoveResetArray[idx] = false;

            player.specialMoveCooldownTimerArray[idx] += Time.deltaTime;

            float duration = activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier);
            if (duration > 0)
                player.specialMoveDurationTimerArray[idx] += Time.deltaTime;

            float cooldown = activeSkillData.cooldown * (1 - player.currentSkillCooldownReducer);

            if (player.specialMoveCooldownTimerArray[idx] > cooldown)
            {
                player.specialMovesCooldownCheckArray[idx] = false;
                player.specialMoveCooldownTimerArray[idx] = 0f;
                player.specialMoveDurationTimerArray[idx] = 0f;
                ResetSpecialMoveCooldownSlot(slotIndex);
            }
        }
    }

    private void SpecialMoveEvent_OnSpecialMoveUsed(SpecialMoveEvent specialMoveEvent, SpecialMoveEventArgs specialMoveEventArgs)
    {
        int index = specialMoveEventArgs.specialMoveNumber - 1;

        ActiveUniqueSkillDetailsSO usedActiveUniqueSkillContainer = player.currentlyUsedActiveUniqueSkills[specialMoveEventArgs.specialMoveNumber];
        int activeSkillLevel = usedActiveUniqueSkillContainer.GetCurrentActiveLevel();
        var activeSkillData = usedActiveUniqueSkillContainer.levels[activeSkillLevel - 1];

        if (player.specialMoveRecastCountArray[index] > 0)
        {
            StopSpecialMoveCoroutine(GetRecastCoroutine(index));
            SetRecastCoroutine(index, StartCoroutine(UpdateRecastCooldownSlotRoutine(specialMoveEventArgs.specialMoveNumber, activeSkillData)));

            // Only reduce if we're not at full count (initial cast)
            int maxCount = activeSkillData.recastRepeatCount;
            if (player.specialMoveRecastCountArray[index] < maxCount)
            {
                ReduceRecastRepeatCount(specialMoveEventArgs.specialMoveNumber);
            }
        }
        else
        {
            ResetSpecialMoveRecastCooldownSlot(specialMoveEventArgs.specialMoveNumber);
            StartNormalCooldown(specialMoveEventArgs.specialMoveNumber);
        }
    }

    private void ReduceRecastRepeatCount(int skillNumber, bool inUpdate = false)
    {
        int index = skillNumber - 1;

        if (player.specialMoveRecastCountArray[index] <= 0 && !inUpdate)
        {
            ResetSpecialMoveRecastCooldownSlot(skillNumber);
            StartNormalCooldown(skillNumber);
        }
    }

    private void StartNormalCooldown(int specialMoveNumber)
    {
        player.specialMovesCooldownCheckArray[specialMoveNumber - 1] = true;
        int index = specialMoveNumber - 1;
        player.specialMoveRecastCountArray[index] = -1;
        StopSpecialMoveCoroutine(GetCooldownCoroutine(index));
        SetCooldownCoroutine(index, StartCoroutine(UpdateCooldownSlotRoutine(specialMoveNumber)));
    }

    private void StopSpecialMoveCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    private Coroutine GetCooldownCoroutine(int index)
    {
        return index switch
        {
            0 => specialMoveOneCooldownCoroutine,
            1 => specialMoveTwoCooldownCoroutine,
            2 => specialMoveThreeCooldownCoroutine,
            _ => null
        };
    }

    private void SetCooldownCoroutine(int index, Coroutine coroutine)
    {
        switch (index)
        {
            case 0: specialMoveOneCooldownCoroutine = coroutine; break;
            case 1: specialMoveTwoCooldownCoroutine = coroutine; break;
            case 2: specialMoveThreeCooldownCoroutine = coroutine; break;
        }
    }

    private Coroutine GetRecastCoroutine(int index)
    {
        return index switch
        {
            0 => specialMoveOneRecastCoroutine,
            1 => specialMoveTwoRecastCoroutine,
            2 => specialMoveThreeRecastCoroutine,
            _ => null
        };
    }

    private void SetRecastCoroutine(int index, Coroutine coroutine)
    {
        switch (index)
        {
            case 0: specialMoveOneRecastCoroutine = coroutine; break;
            case 1: specialMoveTwoRecastCoroutine = coroutine; break;
            case 2: specialMoveThreeRecastCoroutine = coroutine; break;
        }
    }

    private IEnumerator UpdateRecastCooldownSlotRoutine(int specialMoveNumber, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        int index = specialMoveNumber - 1;

        float activeRecastCooldownDuration = activeSkillData.recastCooldown * (1 - player.currentSkillCooldownReducer);

        // Ensure reset flag is false at start
        player.specialMoveRecastCooldownTimerArray[index] = 0f;

        while (player.specialMoveRecastCooldownTimerArray[index] < activeRecastCooldownDuration)
        {
            if (player.specialMoveRecastCountArray[index] > 0)
            {
                Image bgImage = activeSkillSlotContainers[index].GetChild(2).GetComponent<Image>();
                Image fillImage = bgImage.transform.GetChild(0).GetComponent<Image>();

                bgImage.gameObject.SetActive(true);
                float circleFill = Mathf.Clamp01(player.specialMoveRecastCooldownTimerArray[index] / activeRecastCooldownDuration);
                fillImage.fillAmount = 1 - circleFill;
            }

            yield return null;
        }

        // UI only — actual reset will happen from SlotSkillUpdate when count hits 0
        ResetSpecialMoveRecastCooldownSlot(specialMoveNumber);
        player.specialMovesCooldownCheckArray[index] = true;
        StartNormalCooldown(specialMoveNumber);
    }

    private IEnumerator UpdateCooldownSlotRoutine(int specialMoveNumber)
    {
        int index = specialMoveNumber - 1;

        ActiveUniqueSkillDetailsSO usedActiveUniqueSkillContainer = player.currentlyUsedActiveUniqueSkills[specialMoveNumber];
        int activeSkillLevel = usedActiveUniqueSkillContainer.GetCurrentActiveLevel();
        var activeSkillData = usedActiveUniqueSkillContainer.levels[activeSkillLevel - 1];

        float activeCooldownDuration = activeSkillData.cooldown * (1 - player.currentSkillCooldownReducer);

        while (player.specialMoveCooldownTimerArray[index] < activeCooldownDuration)
        {
            if (!specialMoveResetArray[index])
            {
                Image cooldownImage = activeSkillSlotContainers[index].GetChild(1).GetComponent<Image>();
                cooldownImage.gameObject.SetActive(true);
                cooldownImage.fillAmount = 1 - (player.specialMoveCooldownTimerArray[index] / activeCooldownDuration);
            }

            yield return null;
        }
    }

    private void ResetSpecialMoveCooldownSlot(int specialMoveNum)
    {
        int index = specialMoveNum - 1;
        Image cooldownImage = activeSkillSlotContainers[index].GetChild(1).GetComponent<Image>();
        cooldownImage.fillAmount = 1;
        cooldownImage.gameObject.SetActive(false);
        specialMoveResetArray[index] = true;
    }

    private void ResetSpecialMoveRecastCooldownSlot(int specialMoveNum)
    {
        int index = specialMoveNum - 1;
        Image bgImage = activeSkillSlotContainers[index].GetChild(2).GetComponent<Image>();
        Image fillImage = bgImage.transform.GetChild(0).GetComponent<Image>();

        fillImage.fillAmount = 1;
        bgImage.gameObject.SetActive(false);
    }
}