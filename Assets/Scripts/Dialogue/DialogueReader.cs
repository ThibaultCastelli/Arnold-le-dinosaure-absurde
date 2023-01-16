using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueReader : MonoBehaviour
{
    [SerializeField] [Range(0.01f, 1)] float speedLetters = 0.1f;
    [SerializeField] Dialogue dialogue;

    private TextMeshProUGUI text;
    private Queue<string> sentences = new Queue<string>();

    private bool isShowingSentence = false;
    private string lastSentence;

    private void Awake()
    {
        text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        foreach(string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        StartDialogue();
    }

    private void Start()
    {
        InputManager.Instance.Inputs.Player.NextDialogue.performed += ContinueDialogue;
    }

    private void StartDialogue()
    {
        // TODO: animation of the panel

        if (sentences.Count > 0)
        {
            lastSentence = sentences.Dequeue();
            ShowSentence(lastSentence);
        }
    }

    private void ContinueDialogue(InputAction.CallbackContext ctx)
    {
        if(isShowingSentence)
        {
            StopAllCoroutines();
            text.text = lastSentence;
            isShowingSentence = false;
        }
        else if (sentences.Count > 0)
        {
            lastSentence = sentences.Dequeue();
            ShowSentence(lastSentence);
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowSentence(string sentence)
    {
        StopAllCoroutines();
        StartCoroutine(ShowSentenceCoroutine(sentence));
    }

    private IEnumerator ShowSentenceCoroutine(string sentence)
    {
        isShowingSentence = true;
        text.text = "";

        foreach(char c in sentence.ToCharArray())
        {
            text.text += c;
            yield return new WaitForSeconds(speedLetters);
        }

        isShowingSentence = false;
    }

    private void EndDialogue()
    {
        Debug.Log("end of dialogue");
    }
}
