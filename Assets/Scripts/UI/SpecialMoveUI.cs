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
    bool specialMoveOneIsReset;
    bool specialMoveTwoIsReset;
    bool specialMoveThreeIsReset;

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

    private void Update()
    {
        if (player.specialMoveOneOnCooldown)
        {
            specialMoveOneIsReset = false;

            player.specialMoveOneCooldownTimer += Time.deltaTime;

            if (player.playerDetails.specialMoveOneDuration > 0)
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

            if (player.playerDetails.specialMoveTwoDuration > 0)
            {
                player.specialMoveTwoDurationTimer += Time.deltaTime;

                if (player.specialMoveTwoDurationTimer >= player.playerDetails.specialMoveTwoDuration)
                {
                    player.isBlockingActive = false;
                    player.healthEvent.CallArmorWoreOffEvent();
                }
            }

            if (player.specialMoveTwoCooldownTimer > player.playerDetails.specialMoveTwoCooldownDuration)
            {
                // Ensure that the timer is not exceeding the duration
                player.specialMoveTwoOnCooldown = false;
                player.specialMoveTwoCooldownTimer = 0f;
                player.specialMoveTwoDurationTimer = 0f;
                ResetSpecialMoveCooldownSlot(2);
            }
        }

        if (player.specialMoveThreeOnCooldown)
        {
            specialMoveThreeIsReset = false;

            player.specialMoveThreeCooldownTimer += Time.deltaTime;

            if (player.playerDetails.specialMoveThreeDuration > 0)
            {
                player.specialMoveThreeDurationTimer += Time.deltaTime;

                if (player.specialMoveThreeDurationTimer >= player.playerDetails.specialMoveThreeDuration)
                {
                    player.healthEvent.CallGemSkinSpecialMoveEndEvent();
                    Debug.Log("Current armor value is " + GameManager.Instance.GetPlayer().health.currentArmorValue);
                    GameManager.Instance.GetPlayer().health.currentArmorValue = GameManager.Instance.GetPlayer().playerDetails.playerArmorValue; // Reset armor value
                    Debug.Log("Current armor value is " + GameManager.Instance.GetPlayer().health.currentArmorValue);
                }
            }

            if (player.specialMoveThreeCooldownTimer > player.playerDetails.specialMoveThreeCooldownDuration)
            {
                // Ensure that the timer is not exceeding the duration
                player.specialMoveThreeOnCooldown = false;
                player.specialMoveThreeCooldownTimer = 0f;
                player.specialMoveThreeDurationTimer = 0f;
                ResetSpecialMoveCooldownSlot(3);
            }
        }
    }

    private void SpecialMoveEvent_OnSpecialMoveUsed(SpecialMoveEvent specialMoveEvent, SpecialMoveEventArgs specialMoveEventArgs)
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

            //specialMoveCooldownBackground.color = new Color(1f, 1f, 1f);
            //specialMoveImage.color = new Color(0.5f, 0f, 0f);

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
