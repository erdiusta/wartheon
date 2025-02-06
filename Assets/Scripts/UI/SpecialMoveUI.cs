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
    [Tooltip("Populate with the First Special Move object")]
    #endregion Tooltip
    [SerializeField] Transform firstSpecialMoveContainer;
    #region Tooltip
    [Tooltip("Populate with the Second Special Move object")]
    #endregion Tooltip
    [SerializeField] Transform secondSpecialMoveContainer;
    #region Tooltip
    [Tooltip("Populate with the Third Special Move object")]
    #endregion Tooltip
    [SerializeField] Transform thirdSpecialMoveContainer;

    Player player;
    Coroutine specialMoveOneCooldownCoroutine;
    Coroutine specialMoveTwoCooldownCoroutine;
    Coroutine specialMoveThreeCooldownCoroutine;

    float specialMoveOneDuration;
    float specialMoveTwoDuration;
    float specialMoveThreeDuration;

    bool specialMoveOneIsReset;
    bool specialMoveTwoIsReset;
    bool specialMoveThreeIsReset;
    bool skillDurationExpired;
    bool isSkillActive;

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        player.specialMoveEvent.OnSpecialMoveUsed += SpecialMoveEvent_OnSpecialMoveUsed;
    }

    private void OnDisable()
    {
        player.specialMoveEvent.OnSpecialMoveUsed -= SpecialMoveEvent_OnSpecialMoveUsed;
    }

    private void Start()
    {
        // Populate special move imagaes based on selected character
        firstSpecialMoveContainer.GetChild(1).GetComponent<Image>().sprite = player.playerDetails.specialMoveOneImage;
        secondSpecialMoveContainer.GetChild(1).GetComponent<Image>().sprite = player.playerDetails.specialMoveTwoImage;
        thirdSpecialMoveContainer.GetChild(1).GetComponent<Image>().sprite = player.playerDetails.specialMoveThreeImage;
    }

    private void Update()
    {
        if (player.specialMoveOneOnCooldown)
        {
            specialMoveOneIsReset = false;

            player.specialMoveOneCooldownTimer += Time.deltaTime;

            specialMoveOneDuration = player.playerDetails.specialMoveOneDuration;

            if (specialMoveOneDuration > 0)
            {
                player.specialMoveOneDurationTimer += Time.deltaTime;
            }

            if (player.specialMoveOneCooldownTimer > player.playerDetails.specialMoveOneCooldownDuration)
            {
                // Ensure that the timer is not exceeding the duration
                player.specialMoveOneOnCooldown = false;
                player.specialMoveOneCooldownTimer = 0f;
                player.specialMoveOneDurationTimer = 0f;
                ResetSpecialMoveCooldownSlot(1);
            }
        }

        if (player.specialMoveTwoOnCooldown)
        {
            specialMoveTwoIsReset = false;

            player.specialMoveTwoCooldownTimer += Time.deltaTime;

            switch (player.playerDetails.playerCharacterIndex)
            {
                case Character.Astraeus:
                    // Block Skill
                    specialMoveTwoDuration = player.playerDetails.specialMoveTwoDuration * (1 + player.blockSkillAdditionalDurationModifier);
                    break;
                case Character.Erebus:
                    break;
                case Character.Orion:
                    // Lightfeet Skill
                    specialMoveTwoDuration = player.playerDetails.specialMoveTwoDuration * (1 + player.additionalLightfeetSkillDurationModifier);
                    break;
                case Character.Lyrisa:
                    // Barrier Skill
                    specialMoveTwoDuration = player.playerDetails.specialMoveTwoDuration * (1 + player.barrierSkillAdditionalDurationModifier);
                    break;
            }

            if (specialMoveTwoDuration > 0)
            {
                player.specialMoveTwoDurationTimer += Time.deltaTime;

                switch (player.playerDetails.playerCharacterIndex)
                {
                    case Character.Astraeus:
                        if (player.specialMoveTwoDurationTimer >= specialMoveTwoDuration)
                        {
                            player.isBlockingActive = false;
                            player.healthEvent.CallArmorWoreOffEvent();
                        }
                        break;
                    case Character.Erebus:
                        break;
                    case Character.Orion:
                        if (player.specialMoveTwoDurationTimer >= specialMoveTwoDuration && !isSkillActive)
                        {
                            player.movementByVelocity.moveSpeed -= 1f;
                            isSkillActive = true;
                        }
                        break;
                    case Character.Lyrisa:
                        if (player.specialMoveTwoDurationTimer >= specialMoveTwoDuration)
                        {
                            player.forcefieldTransform.gameObject.SetActive(false);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (player.specialMoveTwoCooldownTimer > player.playerDetails.specialMoveTwoCooldownDuration)
            {
                // Ensure that the timer is not exceeding the duration
                player.specialMoveTwoOnCooldown = false;
                player.specialMoveTwoCooldownTimer = 0f;
                isSkillActive = true;
                player.specialMoveTwoDurationTimer = 0f;
                ResetSpecialMoveCooldownSlot(2);
            }
        }

        if (player.specialMoveThreeOnCooldown)
        {
            specialMoveThreeIsReset = false;

            player.specialMoveThreeCooldownTimer += Time.deltaTime;

            specialMoveThreeDuration = player.playerDetails.specialMoveThreeDuration;

            if (specialMoveThreeDuration > 0)
            {
                player.specialMoveThreeDurationTimer += Time.deltaTime;

                if (player.specialMoveThreeDurationTimer >= specialMoveThreeDuration && !skillDurationExpired)
                {
                    skillDurationExpired = true;

                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Astraeus:
                            // Disable status icon on player object
                            player.healthEvent.CallGemSkinSpecialMoveEndEvent();

                            // Reset resistance values
                            if (player.gemSkinBoostGainedDuringGemSkinActive)
                            {
                                player.currentPhysicalResistanceValue -= 0.1f;
                                player.currentFireResistanceValue -= 0.1f;
                                player.currentWaterResistanceValue -= 0.1f;
                                player.currentAirResistanceValue -= 0.1f;
                                player.currentEarthResistanceValue -= 0.1f;
                                player.currentLightResistanceValue -= 0.1f;
                                player.currentDarkResistanceValue -= 0.1f;

                                player.gemSkinBoostGainedDuringGemSkinActive = false;
                            }
                            else
                            {
                                player.currentPhysicalResistanceValue -= (0.1f + player.gemStoneSkillAdditionalModifier);
                                player.currentFireResistanceValue -= (0.1f + player.gemStoneSkillAdditionalModifier);
                                player.currentWaterResistanceValue -= (0.1f + player.gemStoneSkillAdditionalModifier);
                                player.currentAirResistanceValue -= (0.1f + player.gemStoneSkillAdditionalModifier);
                                player.currentEarthResistanceValue -= (0.1f + player.gemStoneSkillAdditionalModifier);
                                player.currentLightResistanceValue -= (0.1f + player.gemStoneSkillAdditionalModifier);
                                player.currentDarkResistanceValue -= (0.1f + player.gemStoneSkillAdditionalModifier);
                            }

                            StaticEventHandler.CallPrimaryStatsChangedEvent();
                            break;
                        case Character.Erebus:
                            break;
                        case Character.Orion:
                            break;
                        case Character.Lyrisa:
                            break;
                        default:
                            break;
                    }
                }
            }

            if (player.specialMoveThreeCooldownTimer > player.playerDetails.specialMoveThreeCooldownDuration)
            {
                // Ensure that the timer is not exceeding the duration
                player.specialMoveThreeOnCooldown = false;
                player.specialMoveThreeCooldownTimer = 0f;
                player.specialMoveThreeDurationTimer = 0f;
                skillDurationExpired = false;
                ResetSpecialMoveCooldownSlot(3);
            }
        }
    }

    private void SpecialMoveEvent_OnSpecialMoveUsed(SpecialMoveEvent specialMoveEvent, SpecialMoveEventArgs specialMoveEventArgs)
    {
        if (specialMoveEventArgs.onlyChangeAlpha)
        {
            Image specialMoveCooldownBackground = firstSpecialMoveContainer.GetChild(0).GetComponent<Image>(); ;
            Image specialMoveCooldownImage = firstSpecialMoveContainer.GetChild(1).GetComponent<Image>();

            // update cooldownCircle
            specialMoveCooldownBackground.fillAmount = 0;
            specialMoveCooldownImage.color = new Color(1f, 1f, 1f, 0.3f);
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
        // Animate the weapon reload bar
        while (player.specialMoveOneCooldownTimer < player.playerDetails.specialMoveOneCooldownDuration)
        {
            Image specialMoveCooldownBackground;
            Image specialMoveCooldownImage;

            switch (specialMoveNumber)
            {
                case 1:
                    if (!specialMoveOneIsReset)
                    {
                        specialMoveCooldownBackground = firstSpecialMoveContainer.GetChild(0).GetComponent<Image>();
                        specialMoveCooldownImage = firstSpecialMoveContainer.GetChild(1).GetComponent<Image>();

                        // update cooldownCircle
                        float circleFill = Mathf.Clamp(player.specialMoveOneCooldownTimer / player.playerDetails.specialMoveOneCooldownDuration, 0, 1);
                        specialMoveCooldownBackground.fillAmount = circleFill;
                        specialMoveCooldownImage.color = new Color(1f, 1f, 1f, 0.3f);
                    }
                    break;
                case 2:
                    if (!specialMoveTwoIsReset)
                    {
                        specialMoveCooldownBackground = secondSpecialMoveContainer.GetChild(0).GetComponent<Image>();
                        specialMoveCooldownImage = secondSpecialMoveContainer.GetChild(1).GetComponent<Image>();

                        // update cooldownCircle
                        float circleFill = Mathf.Clamp(player.specialMoveTwoCooldownTimer / player.playerDetails.specialMoveTwoCooldownDuration, 0, 1);
                        specialMoveCooldownBackground.fillAmount = circleFill;
                        specialMoveCooldownImage.color = new Color(1f, 1f, 1f, 0.3f);
                    }
                    break;
                case 3:
                    if (!specialMoveThreeIsReset)
                    {
                        specialMoveCooldownBackground = thirdSpecialMoveContainer.GetChild(0).GetComponent<Image>();
                        specialMoveCooldownImage = thirdSpecialMoveContainer.GetChild(1).GetComponent<Image>();

                        // update cooldownCircle
                        float circleFill = Mathf.Clamp(player.specialMoveThreeCooldownTimer / player.playerDetails.specialMoveThreeCooldownDuration, 0, 1);
                        specialMoveCooldownBackground.fillAmount = circleFill;
                        specialMoveCooldownImage.color = new Color(1f, 1f, 1f, 0.3f);
                    }
                    break;
                default:
                    break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Reset special move bar coroutine
    private void ResetSpecialMoveCooldownSlot(int specialMoveNum)
    {
        Image specialMoveCooldownBackground;
        Image specialMoveCooldownImage;

        switch (specialMoveNum)
        {
            case 1:
                specialMoveCooldownBackground = firstSpecialMoveContainer.GetChild(0).GetComponent<Image>();
                specialMoveCooldownImage = firstSpecialMoveContainer.GetChild(1).GetComponent<Image>();
                specialMoveCooldownBackground.fillAmount = 1;
                specialMoveCooldownImage.color = new Color(1, 1, 1, 1);
                specialMoveOneIsReset = true;
                break;
            case 2:
                specialMoveCooldownBackground = secondSpecialMoveContainer.GetChild(0).GetComponent<Image>();
                specialMoveCooldownImage = secondSpecialMoveContainer.GetChild(1).GetComponent<Image>();
                specialMoveCooldownBackground.fillAmount = 1;
                specialMoveCooldownImage.color = new Color(1, 1, 1, 1);
                specialMoveTwoIsReset = true;
                break;
            case 3:
                specialMoveCooldownBackground = thirdSpecialMoveContainer.GetChild(0).GetComponent<Image>();
                specialMoveCooldownImage = thirdSpecialMoveContainer.GetChild(1).GetComponent<Image>();
                specialMoveCooldownBackground.fillAmount = 1;
                specialMoveCooldownImage.color = new Color(1, 1, 1, 1);
                specialMoveThreeIsReset = true;
                break;
            default:
                break;
        }
    }
}
