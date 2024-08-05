using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    public List<Dialogue> dialogues;
    public TextMeshPro nameText;
    public TextMeshPro dialogueText;
    public GameObject dialoguePanel;
    public Queue<string> sentences;
    bool dialogueStarted = false;

    private void OnEnable()
    {
        StaticDialogueHandler.OnInsufficientFunds += StaticDialogueHandler_OnInsufficientFunds;
    }

    private void OnDisable()
    {
        StaticDialogueHandler.OnInsufficientFunds -= StaticDialogueHandler_OnInsufficientFunds;
    }

    private void Start()
    {
        sentences = new Queue<string>();
        nameText.color = Color.magenta;
    }

    private void StaticDialogueHandler_OnInsufficientFunds()
    {
        StartDialogue(dialogues[2]);
    }

    public void TriggerDialogue()
    {
        int randomNum = Random.Range(0, 2);

        StartDialogue(dialogues[randomNum]);
    }

    /// <summary>
    /// Trigger to start a dialogue
    /// </summary>
    public void StartDialogue(Dialogue dialogue)
    {
        if (!dialogueStarted)
        {
            dialoguePanel.SetActive(true);
            sentences.Clear();
            dialogueStarted = true;

            foreach (string sentence in dialogue.sentences)
            {
                sentences.Enqueue(sentence);
            }
        }

        nameText.text = dialogue.name;
        DisplayNextSentence();
    }

    /// <summary>
    /// Start next sentence
    /// </summary>
    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.03f);
        }
    }

    /// <summary>
    /// Finalize the dialogue
    /// </summary>
    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueStarted = false;
    }
}
