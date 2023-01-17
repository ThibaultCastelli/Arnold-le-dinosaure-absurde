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
    private Queue<string> _sentences = new Queue<string>();

    private bool _isShowingSentence = false;
    private bool _isFirstDialogue = true;
    private bool _isGamePaused = false;
    private string _lastSentence;

    private void Awake()
    {
        text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        // Enqueue the sentences from the dialogue put in the inspector
        foreach(string sentence in dialogue.sentences)
        {
            _sentences.Enqueue(sentence);
        }

        StartDialogue();
    }

    private void Start()
    {
        InputManager.Instance.Inputs.Player.NextDialogue.performed += ContinueDialogue;
    }

    private void OnEnable()
    {
        Events.OnGamePause += PauseDialogue;
    }

    private void OnDisable()
    {
        Events.OnGamePause -= PauseDialogue;
    }

    private void PauseDialogue(bool isPaused)
    {
        _isGamePaused = isPaused;
    }

    /// <summary>
    /// Make the dialogue box appears and start the animation of the first sentence
    /// </summary>
    private void StartDialogue()
    {
        // TODO: animation of the panel

        // Read the first sentence
        ContinueDialogue(new InputAction.CallbackContext());
    }

    /// <summary>
    /// Show the full sentence by skiping the animation or start the animation of the next sentence.
    /// </summary>
    private void ContinueDialogue(InputAction.CallbackContext ctx)
    {
        // If the animation of the letters is running
        if(_isShowingSentence)
        {
            // Stop the animation and show all the sentence
            StopAllCoroutines();
            text.text = _lastSentence;
            _isShowingSentence = false;
        }
        else if (_sentences.Count > 0)
        {
            // Else start the animation of the next sentence
            _lastSentence = _sentences.Dequeue();
            StopAllCoroutines();
            StartCoroutine(ShowSentenceCoroutine(_lastSentence));

            // Start to invoke cactus after the first dialogue is passed
            if (_isFirstDialogue)
            {
                _isFirstDialogue = false;
                Events.OnFirstDialoguePass?.Invoke();
            }
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// Animation of the sentence (letters appearing one by one).
    /// </summary>
    /// <param name="sentence">The sentence to animate.</param>
    private IEnumerator ShowSentenceCoroutine(string sentence)
    {
        _isShowingSentence = true;
        // Reset the text field of the dialogue box
        text.text = ""; 

        // Add letters one by one
        foreach(char c in sentence.ToCharArray())
        {
            text.text += c;
            yield return new WaitForSeconds(speedLetters);

            // Infinite loop while the game is paused
            while(_isGamePaused)
            {
                yield return new WaitForSeconds(speedLetters);
            }
        }

        // Indiquate that the animation is over
        _isShowingSentence = false;
    }

    private void EndDialogue()
    {
        Debug.Log("end of dialogue");
    }
}
