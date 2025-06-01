using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    public List<Dialogue> dialogues;

    TextMeshProUGUI nameText;
    TextMeshProUGUI dialogueText;
    GameObject topLetterboxPanel;
    GameObject bottomLetterboxPanel;

    public Queue<string> sentences;
    bool dialogueStarted = false;

    NPC npc;

    private void OnEnable()
    {
        if (CinematicSceneManager.Instance != null) // Cinmatic talk
        {
            nameText = CinematicSceneManager.Instance.nameText;
            dialogueText = CinematicSceneManager.Instance.dialogueText;
        }
        else
        {
            topLetterboxPanel = GameManager.Instance.topBar.gameObject;
            bottomLetterboxPanel = GameManager.Instance.bottomBar.gameObject;
            nameText = topLetterboxPanel.GetComponentInChildren<TextMeshProUGUI>();
            dialogueText = bottomLetterboxPanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        StaticDialogueHandler.OnInsufficientFunds += StaticDialogueHandler_OnInsufficientFunds;
        StaticDialogueHandler.OnGambleLost += StaticDialogueHandler_OnGambleLost;
        StaticDialogueHandler.OnGambleWon += StaticDialogueHandler_OnGambleWon;
        StaticDialogueHandler.OnTradeCompleted += StaticDialogueHandler_OnTradeCompleted;

        StaticDialogueHandler.OnMoldranTalk += StaticDialogueHandler_OnMoldranTalk;
    }

    private void OnDisable()
    {
        StaticDialogueHandler.OnInsufficientFunds -= StaticDialogueHandler_OnInsufficientFunds;
        StaticDialogueHandler.OnGambleLost -= StaticDialogueHandler_OnGambleLost;
        StaticDialogueHandler.OnGambleWon -= StaticDialogueHandler_OnGambleWon;
        StaticDialogueHandler.OnTradeCompleted -= StaticDialogueHandler_OnTradeCompleted;

        StaticDialogueHandler.OnMoldranTalk -= StaticDialogueHandler_OnMoldranTalk;
    }

    private void Start()
    {
        sentences = new Queue<string>();

        npc = GetComponent<NPC>();
    }

    private void StaticDialogueHandler_OnTradeCompleted()
    {
        npc.tradeDone = true;
    }

    private void StaticDialogueHandler_OnMoldranTalk(MoldranDialogueEventArgs moldranDialogueEventArgs)
    {
        StartDialogue(dialogues[moldranDialogueEventArgs.dialogueNumber], moldranTalk: true, moldranDialogueEventArgs.moldranSpeechOrder);
    }

    private void StaticDialogueHandler_OnInsufficientFunds()
    {
        StartDialogue(dialogues[2]);
    }

    private void StaticDialogueHandler_OnGambleWon()
    {
        EndDialogue();
        StartDialogue(dialogues[3]);
    }

    private void StaticDialogueHandler_OnGambleLost()
    {
        EndDialogue();
        StartDialogue(dialogues[2]);
    }

    public void TriggerDialogue()
    {
        if (npc.tradeDone)
        {
            if (npc.hintGiven)
            {
                StartDialogue(dialogues[3]);
            }
            else
            {
                int randomNum = Random.Range(4, dialogues.Count);
                StartDialogue(dialogues[randomNum]);
                npc.hintGiven = true;
            }
        }
        else
        {
            int randomNum = Random.Range(0, 2);

            StartDialogue(dialogues[randomNum]);
        }
    }

    /// <summary>
    /// Trigger to start a dialogue
    /// </summary>
    public void StartDialogue(Dialogue dialogue, bool moldranTalk = false, MoldranSpeechOrder moldranSpeechOrder = MoldranSpeechOrder.firstSpeech)
    {
        if (!dialogueStarted)
        {
            sentences.Clear();
            dialogueStarted = true;

            foreach (string sentence in dialogue.sentences)
            {
                sentences.Enqueue(sentence);
            }
        }

        nameText.text = dialogue.name; // Title text

        if (moldranTalk)
        {
            StartCoroutine(AutoPlayDialogueCoroutine(moldranSpeechOrder));
        }
        else
        {
            DisplayNextSentence(moldranTalk);
        }
    }

    /// <summary>
    /// Auto play dialogue
    /// </summary>
    private IEnumerator AutoPlayDialogueCoroutine(MoldranSpeechOrder moldranSpeechOrder = MoldranSpeechOrder.firstSpeech)
    {
        yield return null; // Wait one frame to allow UI layout to appear

        while (sentences.Count > 0)
        {
            string sentence = sentences.Dequeue();
            dialogueText.text = "";

            yield return null;

            foreach (char letter in sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(0.05f); // typing speed
            }

            Debug.Log(dialogueText.text);

            yield return new WaitForSeconds(2f); // wait after sentence
        }

        switch (moldranSpeechOrder)
        {
            case MoldranSpeechOrder.firstSpeech:
                CinematicSceneManager.Instance.cinematicPhase = CinematicPhase.riftOpening;
                break;
            case MoldranSpeechOrder.secondSpeech:
                CinematicSceneManager.Instance.cinematicPhase = CinematicPhase.closingScene;
                break;
            default:
                break;
        }

        // End of all sentences
        EndDialogue(moldranTalk: true);
    }

    /// <summary>
    /// Start next sentence
    /// </summary>
    public void DisplayNextSentence(bool moldranTalk = false)
    {
        if (sentences.Count == 0)
        {
            EndDialogue(moldranTalk);
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence, moldranTalk));
    }

    IEnumerator TypeSentence(string sentence, bool moldranTalk = false)
    {
        dialogueText.text = "";

        // Force layout update before writing starts
        Canvas.ForceUpdateCanvases();

        yield return null;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// Finalize the dialogue
    /// </summary>
    void EndDialogue(bool moldranTalk = false)
    {
        dialogueStarted = false;
        dialogueText.text = "";

        // Optional: Do something extra if it's Moldran's cinematic dialogue
        if (moldranTalk)
        {
            // For example, trigger a follow-up event or cutscene
            StaticDialogueHandler.CallMoldranFinishedEvent();
        }
    }
}
