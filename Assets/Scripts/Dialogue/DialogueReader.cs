using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class DialogueReader : MonoBehaviour
{
    [SerializeField] [Range(0.01f, 1)] float speedLetters = 0.1f;
    [SerializeField] Dialogue dialogue;

    private TextMeshProUGUI text;
    private Queue<string> _sentences = new Queue<string>();

    private bool _isShowingSentence = false;
    private bool _isFirstDialogue = true;
    private bool _isGamePaused = false;
    private bool _isGameOver = false;
    private bool _isGameRestart = false;

    private string _currSentence;
    private string _sentenceBeforeGameover;

    private int dialoguePassedCount = 0;

    private RectTransform rectTransform;

    private void Awake()
    {
        // Get components
        text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        // Enqueue the sentences from the dialogue put in the inspector
        foreach(string sentence in dialogue.sentences)
        {
            _sentences.Enqueue(sentence);
        } 
    }

    private void Start()
    {
        InputManager.Instance.Inputs.Player.NextDialogue.performed += ContinueDialogue;
    }

    private void OnEnable()
    {
        Events.OnGamePause += PauseDialogue;
        Events.OnGameStart += StartDialogue;
        Events.OnGameOver += GameOverDialogue;
        Events.OnGameRestart += RestartDialogue;
        Events.OnGameReload += Reload;
    }

    private void OnDisable()
    {
        Events.OnGamePause -= PauseDialogue;
        Events.OnGameStart -= StartDialogue;
        Events.OnGameOver -= GameOverDialogue;
        Events.OnGameRestart -= RestartDialogue;
        Events.OnGameReload -= Reload;
    }

    private void Reload()
    {
        InputManager.Instance.Inputs.Player.NextDialogue.performed -= ContinueDialogue;
    }

    /// <summary>
    /// Show a game over sentence and prevent to show more dialogue
    /// </summary>
    private void GameOverDialogue()
    {
        // Stop current sentence and sound
        StopAllCoroutines();
        SoundManager.Instance.StopSfxControlLoop("DialogueVoice");

        // Show game over sentence
        StartCoroutine(ShowSentenceCoroutine("Parfois, comme maintenant, je cède, et j'aimerais que cela s'arrête."));

        _isGameOver = true;
        _sentenceBeforeGameover = _currSentence;
    }

    /// <summary>
    /// Show restart dialogue and re-enable to show more dialogues
    /// </summary>
    private void RestartDialogue()
    {
        // Stop current sentence and sound
        StopAllCoroutines();
        SoundManager.Instance.StopSfxControlLoop("DialogueVoice");

        // Show restart sentence
        _currSentence = "Mais qu'importe, reprenons.";
        StartCoroutine(ShowSentenceCoroutine(_currSentence));

        _isGameOver = false;
        _isFirstDialogue = true;
        _isGameRestart = true;
    }

    /// <summary>
    /// Show or hide the dialogue panel when the game is paused or unpaused.
    /// </summary>
    /// <param name="isPaused">True if the game is paused, otherwise false.</param>
    private void PauseDialogue(bool isPaused)
    {
        _isGamePaused = isPaused;

        if (isPaused)
        {
            rectTransform.position = new Vector3(rectTransform.position.x, -300, rectTransform.position.z);
        }
        else
        {
            rectTransform.position = new Vector3(rectTransform.position.x, 20, rectTransform.position.z);
        }
    }

    /// <summary>
    /// Make the dialogue box appears and start the animation of the first sentence
    /// </summary>
    private void StartDialogue()
    {
        // Animation in
        LeanTween.moveY(gameObject, 20, 1).setEaseInOutCubic();

        // Read the first sentence
        _currSentence = _sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(ShowSentenceCoroutine(_currSentence));
    }

    /// <summary>
    /// Show the full sentence by skiping the animation or start the animation of the next sentence.
    /// </summary>
    private void ContinueDialogue(InputAction.CallbackContext ctx)
    {
        // If the animation of the letters is running
        if(_isShowingSentence && !_isGameOver)
        {
            // Stop the animation and sound
            StopAllCoroutines();
            SoundManager.Instance.StopSfxControlLoop("DialogueVoice");

            // Show all the sentence
            text.text = _currSentence;

            _isShowingSentence = false;
        }
        else if (_sentences.Count > 0 && !_isGameOver)
        {
            // Stop sentence animation
            StopAllCoroutines();

            if (_isGameRestart)
            {
                // Show the sentence before game over
                StartCoroutine(ShowSentenceCoroutine(_sentenceBeforeGameover));
                _currSentence = _sentenceBeforeGameover;
                _isGameRestart = false;
            }
            else
            {
                // Show next sentence
                _currSentence = _sentences.Dequeue();
                StartCoroutine(ShowSentenceCoroutine(_currSentence));

                // Accelerate the game every two dialogues
                dialoguePassedCount++;
                if (dialoguePassedCount % 2 == 0)
                {
                    Events.OnAcceleration?.Invoke(0.5f);
                }
            }
            
            // Start to invoke cactus after the first dialogue is passed
            if (_isFirstDialogue)
            {
                _isFirstDialogue = false;
                Events.OnFirstDialoguePass?.Invoke();
            }

            // Reach end of dialogue
            if (_sentences.Count == 0)
            {
                Events.OnGameEnding?.Invoke(); 
            }
        }
    }

    /// <summary>
    /// Animation of the sentence (letters appearing one by one).
    /// </summary>
    /// <param name="sentence">The sentence to animate.</param>
    private IEnumerator ShowSentenceCoroutine(string sentence)
    {
        // Indiquate that the animation is running
        _isShowingSentence = true;

        SoundManager.Instance.PlaySfxControlLoop("DialogueVoice", 0.6f);

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

        SoundManager.Instance.StopSfxControlLoop("DialogueVoice");
    }

    private void EndDialogue()
    {
        Debug.Log("end of dialogue");
    }
}
