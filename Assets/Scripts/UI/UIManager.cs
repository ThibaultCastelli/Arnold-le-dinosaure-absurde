using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Main menu")]
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject mainMenuTitle;
    [SerializeField] GameObject mainMenuBtns; 
    [SerializeField] GameObject mainMenuOptions;
    [SerializeField] GameObject mainMenuControls;
    [SerializeField] GameObject mainMenuCredits;
    [SerializeField] GameObject backBtn;
    [SerializeField] GameObject backBtnCredits;
    [SerializeField] GameObject playBtn;
    [SerializeField] Slider sliderVolumeMainMenu;
    [SerializeField] Image sliderImageMainMenu;
    [SerializeField] TextMeshProUGUI fullscreenTxtMainMenu;

    [Header("Pause menu")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject pauseMenuFirstSelected;
    [SerializeField] Image sliderImagePauseMenu;
    [SerializeField] Slider sliderVolumePauseMenu;
    [SerializeField] TextMeshProUGUI fullscreenTxtPauseMenu;

    [Header("Gameover menu")]
    [SerializeField] GameObject gameoverMenu;
    [SerializeField] GameObject restartBtn;

    [Header("Ending menu")]
    [SerializeField] GameObject endingMenu;
    [SerializeField] GameObject replayBtn;

    [Header("Other")]
    [SerializeField] EventSystem eventSystem;

    private bool _isPaused = false;
    private bool _isOnOptions = false;  // Used in main menu
    private bool _isOnCredits = false;  // Used in main menu

    private Color _originalSliderColor;
    private Color _originalFullscreenTxtColor;

    private void Awake()
    {
        // Get original colors
        _originalSliderColor = sliderImagePauseMenu.color;
        _originalFullscreenTxtColor = fullscreenTxtPauseMenu.color;

        // Set default selected button
        eventSystem.SetSelectedGameObject(playBtn);

        // Animation of main menu on start of the game
        LeanTween.moveLocalX(mainMenuBtns, 0, 0.7f).setEaseOutBack();
        LeanTween.moveLocal(mainMenuControls, new Vector3(600, -24, 0), 0.7f).setEaseOutBack();
        LeanTween.moveLocalY(mainMenuTitle, 500, 0.7f).setEaseOutBack();
    }

    private void Start()
    {
        // Default value for the slider volume
        float savedVolume = PlayerPrefs.GetFloat("volume", 0.66f);
        sliderVolumePauseMenu.value = savedVolume;
        sliderVolumeMainMenu.value = savedVolume;

        InputManager.Instance.Inputs.UI.Cancel.performed += EscKey;
    }

    private void OnEnable()
    {
        Events.OnGameOver += ShowGameOver;
        Events.OnGameEnding += ShowEndingMenu;
    }
    private void OnDisable()
    {
        Events.OnGameOver -= ShowGameOver;
        Events.OnGameEnding -= ShowEndingMenu;
    }

    private void EscKey(InputAction.CallbackContext ctx)
    {
        if(_isOnOptions || _isOnCredits)
        {
            Back();
        }
    }

    /// <summary>
    /// Triggered when the game is ending.
    /// </summary>
    private void ShowEndingMenu()
    {
        LeanTween.moveLocalX(endingMenu, 0, 1).setEaseOutBack();
        eventSystem.SetSelectedGameObject(null);
        StartCoroutine(SelectReplayBtn());
    }
    private IEnumerator SelectReplayBtn()
    {
        yield return new WaitForSeconds(2);
        eventSystem.SetSelectedGameObject(replayBtn);
    }

    /// <summary>
    /// Function for "rejouer" button.
    /// </summary>
    public void Replay()
    {
        Events.OnGameReload?.Invoke();
        // Reload the scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        InputManager.Instance.Inputs.Player.Disable();
        InputManager.Instance.Inputs.Player.Pause.performed -= Pause;
        InputManager.Instance.Inputs.UI.Cancel.performed -= Pause;
    }

    /// <summary>
    /// Function for "r?essayer" button.
    /// </summary>
    public void Restart()
    {
        // Animation out of "rejouer" button
        LeanTween.moveLocalX(gameoverMenu, 1230, 1).setEaseOutBack();

        // Deselect button
        eventSystem.SetSelectedGameObject(null);

        // Remove low pass on music
        SoundManager.Instance.MusicLowPass(false);

        Events.OnGameRestart?.Invoke();
    }

    /// <summary>
    /// Triggered by OnGameOver event. Show the "rejouer" button.
    /// </summary>
    private void ShowGameOver()
    {
        // Animation in of "rejouer" button
        LeanTween.moveLocalX(gameoverMenu, 0, 1).setEaseOutBack();
        // Select "rejouer" button
        eventSystem.SetSelectedGameObject(restartBtn);
    }

    /// <summary>
    /// Function for the "joueur" button.
    /// </summary>
    public void Play()
    {
        // Animation out of main menu
        LeanTween.moveLocalY(mainMenu, 1100, 1).setEaseInCubic();
        LeanTween.moveLocalX(mainMenuCredits, 2000, 0.5f);
        
        Events.OnGameStart?.Invoke();

        InputManager.Instance.Inputs.UI.Disable();
        eventSystem.SetSelectedGameObject(null);

        // Change the esc button to be the pause button
        InputManager.Instance.Inputs.UI.Cancel.performed -= EscKey;
        InputManager.Instance.Inputs.Player.Pause.performed += Pause;
        InputManager.Instance.Inputs.UI.Cancel.performed += Pause;
    }

    /// <summary>
    /// Function for the "options" button on the main menu.
    /// </summary>
    public void GoToOptions()
    {
        // Animation
        LeanTween.moveLocalX(mainMenuBtns, -2000, 0.4f).setEaseInBack();
        LeanTween.moveLocalX(mainMenuOptions, 5, 0.8f).setEaseInOutBack();
        LeanTween.moveLocal(mainMenuControls, new Vector3(-550, -380, 0), 0.7f).setEaseInOutCirc();

        eventSystem.SetSelectedGameObject(backBtn);
        _isOnOptions = true;
    }

    /// <summary>
    /// Function for the "credits" button on the main menu.
    /// </summary>
    public void GoToCredits()
    {
        // Animation
        LeanTween.moveLocalX(mainMenuBtns, -2000, 0.4f).setEaseInBack();
        LeanTween.moveLocalX(mainMenuControls, 1600, 0.4f).setEaseInBack();
        LeanTween.moveLocalY(mainMenuCredits, -190, 0.6f).setEaseOutQuart();

        eventSystem.SetSelectedGameObject(backBtnCredits);
        _isOnCredits = true;
    }

    /// <summary>
    /// Function for the "retour" button on the main menu.
    /// </summary>
    public void Back()
    {
        // Animation
        LeanTween.moveLocalX(mainMenuBtns, 0, 0.7f).setEaseOutBack();
        LeanTween.moveLocalX(mainMenuOptions, 630, 0.2f).setEaseInBack();
        LeanTween.moveLocal(mainMenuControls, new Vector3(600, -24, 0), 0.7f).setEaseOutBack();
        LeanTween.moveLocalY(mainMenuCredits, -1200, 0.4f).setEaseInBack();

        eventSystem.SetSelectedGameObject(playBtn);
        _isOnCredits = false;
        _isOnOptions = false;
    }

    /// <summary>
    /// Function for the "Continuer" button.
    /// </summary>
    public void Continue()
    {
        Pause(new InputAction.CallbackContext());
    }

    /// <summary>
    /// Function for the "Quitter" button.
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Function for the audio volume slider.
    /// </summary>
    /// <param name="volume"></param>
    public void SetVolumeMaster(float volume)
    {
        Events.OnVolumeChange(MyMath.CustomRange(volume, 0, 1, -40, 5));
        PlayerPrefs.SetFloat("volume", volume);
    }

    /// <summary>
    /// Function for the "Plein ?cran" toggle.
    /// </summary>
    /// <param name="fullscreen"></param>
    public void Fullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    public void HighlightFullscreen()
    {
        // Highlight fullscreen text
        fullscreenTxtPauseMenu.color = Color.white;
        fullscreenTxtMainMenu.color = Color.white;
    }
    public void DelightFullscreen()
    {
        // Delight fullscreen text
        fullscreenTxtPauseMenu.color = _originalFullscreenTxtColor;
        fullscreenTxtMainMenu.color = _originalFullscreenTxtColor;
    }

    public void HighlightAudioVolume()
    {
        // Highlight audio image
        sliderImagePauseMenu.color = Color.white;
        sliderImageMainMenu.color = Color.white;
    }
    public void DelightAudioVolume()
    {
        // Delight audio image
        sliderImagePauseMenu.color = _originalSliderColor;
        sliderImageMainMenu.color = _originalSliderColor;
    }

    /// <summary>
    /// Show or hide the pause menu.
    /// </summary>
    private void Pause(InputAction.CallbackContext ctx)
    {
        _isPaused = !_isPaused;
        Events.OnGamePause?.Invoke(_isPaused);

        if(_isPaused)
        {
            // Show pause menu
            pauseMenu.SetActive(true);
            eventSystem.SetSelectedGameObject(pauseMenuFirstSelected);

            InputManager.Instance.Inputs.Player.Disable();
            InputManager.Instance.Inputs.UI.Enable();
        }
        else
        {
            // Hide pause menu
            pauseMenu.SetActive(false);

            InputManager.Instance.Inputs.UI.Disable();
            InputManager.Instance.Inputs.Player.Enable();
        }
    }
}
