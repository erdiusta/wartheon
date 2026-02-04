using UnityEngine;

public class Dummy : MonoBehaviour
{
    public SoundEffectSO dummyHitSound;
    public Health health;
    public HealthEvent healthEvent;
    public Transform promptArrowContainer;

    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public DestroyedEvent destroyedEvent;

    private void Awake()
    {
        health = GetComponent<Health>();
        healthEvent = GetComponent<HealthEvent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        destroyedEvent = GetComponent<DestroyedEvent>();
    }

    private void Start()
    {
        if (tag == Settings.decoyTag )
        {
            health.SetMaximumHealth(30);

            SceneObjectsManager.dynamicGameObjectsInScene.Add(gameObject);
        }
    }

    private void Update()
    {
        if (InputManager.TutorialEnabled && tag == Settings.practiceDummy)
        {
            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.AimAndFire)
            {
                promptArrowContainer.gameObject.SetActive(true);

                if (health.damageTaken)
                {
                    TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed; // Tutorial passed
                }
            }
            else
            {
                promptArrowContainer.gameObject.SetActive(false);
            }
        }
    }

    public Vector3 GetDecoyPosition() => transform.position + new Vector3(0f, 0.7f, 0f);
}
