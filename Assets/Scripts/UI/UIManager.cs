using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pause menu")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject pauseMenuFirstSelected;
    [SerializeField] GameObject pauseMenuSlider;
    [SerializeField] Image sliderImage;
    [SerializeField] Slider sliderVolume;
    [SerializeField] GameObject pauseMenuToggle;
    [SerializeField] TextMeshProUGUI fullscreenTxt;

    [Header("Other")]
    [SerializeField] EventSystem eventSystem;

    private bool _isPaused = false;

    private Color _originalSliderColor;
    private Color _originalFullscreenTxtColor;

    private void Awake()
    {
        _originalSliderColor = sliderImage.color;
        _originalFullscreenTxtColor = fullscreenTxt.color;
    }

    private void Start()
    {
        InputManager.Instance.Inputs.Player.Pause.performed += Pause;
        InputManager.Instance.Inputs.UI.Cancel.performed += Pause;

        // Default value for the slider
        // TODO: save the slider volume to playerprefs and get it on start
        sliderVolume.value = 0.66f;
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
        Debug.Log("Quit");
    }

    /// <summary>
    /// Function for the audio volume slider.
    /// </summary>
    /// <param name="volume"></param>
    public void SetVolumeMaster(float volume)
    {
        Events.OnVolumeChange(MyMath.CustomRange(volume, 0, 1, -40, 20));
    }

    /// <summary>
    /// Function for the "Plein écran" toggle.
    /// </summary>
    /// <param name="fullscreen"></param>
    public void Fullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    public void HighlightFullscreen() => fullscreenTxt.color = Color.white;
    public void DelightFullscreen() => fullscreenTxt.color = _originalFullscreenTxtColor;

    public void HighlightAudioVolume() => sliderImage.color = Color.white;
    public void DelightAudioVolume() => sliderImage.color = _originalSliderColor;

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
