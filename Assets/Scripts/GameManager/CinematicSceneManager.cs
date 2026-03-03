using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class CinematicSceneManager : SingletonMonobehaviour<CinematicSceneManager>
{
    [SerializeField] Light2D light2D;
    [SerializeField] GameObject moldranObject;
    [SerializeField] GameObject riftObject;
    [SerializeField] SoundEffectSO riftOpenSoundEffect;

    // Cinematic dialogue box paneþ
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [HideInInspector] public  CinematicPhase cinematicPhase = CinematicPhase.openingScene;

    Animator moldranAnimator;
    Animator riftAnimator;

    Transform riftMobContainerTransform;
    RiftMobContainer riftMobContainer;
    Coroutine mobSpillCoroutine;
    Vector2[] directions = new Vector2[] { Vector2.right, Vector2.down, Vector2.left };
    float mobMoveSpeed = 3.5f;

    float fadeInDelay = 4f;
    float fadeInDuration = 6f;
    float fadeInTimer = 0f;
    float riftOpenTimer = 0f;
    bool fadeInCompleted;
    bool riftAnimationRun;
    bool riftOpened = false; // Used for playing open rift sound

    float fadeOutTimer = 0f;
    float fadeOutDuration = 5f;
    bool fadeOutCompleted;
    bool finalSpeechTriggered;


    private void Start()
    {
        moldranAnimator = moldranObject.GetComponent<Animator>();
        riftAnimator = riftObject.GetComponent<Animator>();
        riftMobContainerTransform = riftObject.transform.GetChild(0);
        riftMobContainer = riftMobContainerTransform.GetComponent<RiftMobContainer>();

        // Play music
        MusicManager.Instance.PlayMusic(GameResources.Instance.cutsceneMusic);
    }

    private void Update()
    {
        if (InputManager.Instance.OKButton.action.WasPressedThisFrame() || InputManager.Instance.escapeButton.action.WasPressedThisFrame())
        {
            SceneManager.LoadScene("MainMenuScene");
        }

        if (riftOpened && riftOpenSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(riftOpenSoundEffect);
            riftOpened = false;
        }

        switch (cinematicPhase)
        {
            case CinematicPhase.openingScene:

                if (fadeInCompleted) return;

                fadeInTimer += Time.deltaTime;

                FadeInProcess();
                TriggerSpeechBeforeOpeningRift();
                break;

            case CinematicPhase.riftOpening:

                OpenRift();
                break;

            case CinematicPhase.mobSpilledFromRift:

                if (mobSpillCoroutine == null)
                {
                    mobSpillCoroutine = StartCoroutine(SpawnMobsFromRiftCoroutine());
                }

                break;

            case CinematicPhase.finalSpeech:

                if (!finalSpeechTriggered)
                {
                    moldranAnimator.SetBool("openRift", false);
                    TriggerFinalSpeech();
                    finalSpeechTriggered = true;
                }

                break;

            case CinematicPhase.closingScene:

                if (fadeOutCompleted)
                {
                    SceneManager.LoadScene("MainMenuScene");
                }

                fadeOutTimer += Time.deltaTime;
                FadeOutProcess();
          
                break;

            default:
                break;
        }
    }

    IEnumerator SpawnMobsFromRiftCoroutine()
    {
        int dirIndex = 0;

        for (int i = 0; i < riftMobContainer.spillingMobs.Count; i++)
        {
            GameObject mob = Instantiate(riftMobContainer.spillingMobs[i], riftMobContainerTransform.position, Quaternion.identity, riftMobContainerTransform);
            AdjustMobAnimationDirection(ref mob, dirIndex);

            Vector2 direction = directions[dirIndex];
            dirIndex = (dirIndex + 1) % directions.Length;

            // Move the mob manually (either via Rigidbody2D or transform)
            Rigidbody2D rb = mob.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * mobMoveSpeed;
            }

            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1.5f); // Time interval before final speech of Moldran

        cinematicPhase = CinematicPhase.finalSpeech;
    }

    private void AdjustMobAnimationDirection(ref GameObject mob, int dirIndex)
    {
        Animator mobAnimator = mob.GetComponent<Animator>();
        mobAnimator.SetFloat(Settings.motionType, 1f);

        // Set aim direction
        switch (dirIndex)
        {
            case 0:
                mobAnimator.SetBool(Settings.aimRight, true);
                mobAnimator.SetFloat(Settings.axisX, 1f);
                mobAnimator.SetFloat(Settings.axisY, 0f);
                break;

            case 1:
                mobAnimator.SetBool(Settings.aimDown, true);
                mobAnimator.SetFloat(Settings.axisX, 0f);
                mobAnimator.SetFloat(Settings.axisY, -1f);
                break;

            case 2:
                mobAnimator.SetBool(Settings.aimLeft, true);
                mobAnimator.SetFloat(Settings.axisX, -1f);
                mobAnimator.SetFloat(Settings.axisY, 0f);
                break;

        }
    }

    private void FadeInProcess()
    {
        if (fadeInTimer <= fadeInDelay)
        {
            // Stay at 0 intensity during initial delay
            light2D.intensity = 0f;
        }
        else
        {
            float fadeProgress = Mathf.Clamp01((fadeInTimer - fadeInDelay) / fadeInDuration);
            light2D.intensity = fadeProgress;

            if (fadeProgress >= 1f)
            {
                fadeInCompleted = true;
            }
        }
    }

    private void FadeOutProcess()
    {
        if (fadeOutTimer <= fadeOutDuration)
        {
            float fadeProgress = Mathf.Clamp01((fadeOutDuration - fadeOutTimer) / fadeOutDuration);
            light2D.intensity = fadeProgress;
        }
        else
        {
            fadeOutCompleted = true;
        }
    }

    private void TriggerSpeechBeforeOpeningRift()
    {
        if (!riftAnimationRun && fadeInTimer > 9f)
        {
            Invoke(nameof(TriggerFirstSpeech), 3);

            StaticDialogueHandler.CallMoldranTalkEvent(0, MoldranSpeechOrder.firstSpeech); // First speech
            riftAnimationRun = true;
        }
    }

    private void TriggerFinalSpeech()
    {
        StaticDialogueHandler.CallMoldranTalkEvent(1, MoldranSpeechOrder.secondSpeech); // Final speech
    }

    private void TriggerFirstSpeech()
    {
        moldranAnimator.SetBool("openRift", true);
    }

    private void OpenRift()
    {
        riftAnimator.SetTrigger("openRift");
        riftOpened = true;

        riftOpenTimer += Time.deltaTime;

        if (riftOpenTimer > 3f)
        {
            cinematicPhase = CinematicPhase.riftOpening;
        }
    }
}
