using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject optionsPanel;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [SerializeField] private Toggle fullscreenToggle;

    [SerializeField] private Slider uiScaleSlider;
    [SerializeField] private RectTransform[] uiElementsToScale;

    [SerializeField] private float minUIScale = 0.9f;
    [SerializeField] private float maxUIScale = 1.1f;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string FullscreenKey = "Fullscreen";
    private const string UIScaleKey = "UIScale";

    private Vector3[] originalScales;

    private void Awake()
    {
        CacheOriginalScales();
        ConfigureSliderLimits();
    }

    private void Start()
    {
        LoadSettings();
        ConfigureListeners();
    }

    private void CacheOriginalScales()
    {
        if (uiElementsToScale == null) return;

        originalScales = new Vector3[uiElementsToScale.Length];

        for (int i = 0; i < uiElementsToScale.Length; i++)
        {
            if (uiElementsToScale[i] != null)
            {
                originalScales[i] = uiElementsToScale[i].localScale;
            }
            else
            {
                originalScales[i] = Vector3.one;
            }
        }
    }

    private void ConfigureSliderLimits()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0.0001f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.wholeNumbers = false;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0.0001f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.wholeNumbers = false;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0.0001f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.wholeNumbers = false;
        }

        if (uiScaleSlider != null)
        {
            uiScaleSlider.minValue = minUIScale;
            uiScaleSlider.maxValue = maxUIScale;
            uiScaleSlider.wholeNumbers = false;
        }
    }

    private void ConfigureListeners()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        if (uiScaleSlider != null)
        {
            uiScaleSlider.onValueChanged.AddListener(SetUIScale);
        }
    }

    private void LoadSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
        bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        float uiScale = PlayerPrefs.GetFloat(UIScaleKey, 1f);

        masterVolume = Mathf.Clamp(masterVolume, 0.0001f, 1f);
        musicVolume = Mathf.Clamp(musicVolume, 0.0001f, 1f);
        sfxVolume = Mathf.Clamp(sfxVolume, 0.0001f, 1f);
        uiScale = Mathf.Clamp(uiScale, minUIScale, maxUIScale);

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(masterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
        }

        if (uiScaleSlider != null)
        {
            uiScaleSlider.SetValueWithoutNotify(uiScale);
        }

        ApplyMasterVolume(masterVolume);
        ApplyMusicVolume(musicVolume);
        ApplySFXVolume(sfxVolume);
        ApplyFullscreen(fullscreen);
        ApplyUIScale(uiScale);
    }

    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);

        ApplyMasterVolume(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);

        ApplyMusicVolume(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);

        ApplySFXVolume(value);
        PlayerPrefs.SetFloat(SFXVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        ApplyFullscreen(isFullscreen);
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetUIScale(float value)
    {
        value = Mathf.Clamp(value, minUIScale, maxUIScale);

        ApplyUIScale(value);
        PlayerPrefs.SetFloat(UIScaleKey, value);
        PlayerPrefs.Save();
    }

    private void ApplyMasterVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }
    }

    private void ApplyMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    private void ApplySFXVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    private void ApplyFullscreen(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreen = isFullscreen;
    }

    private void ApplyUIScale(float value)
    {
        value = Mathf.Clamp(value, minUIScale, maxUIScale);

        if (uiElementsToScale == null) return;
        if (originalScales == null || originalScales.Length != uiElementsToScale.Length)
        {
            CacheOriginalScales();
        }

        for (int i = 0; i < uiElementsToScale.Length; i++)
        {
            RectTransform element = uiElementsToScale[i];

            if (element == null) continue;

            if (optionsPanel != null)
            {
                if (element == optionsPanel.transform || element.IsChildOf(optionsPanel.transform))
                {
                    continue;
                }
            }

            element.localScale = originalScales[i] * value;
        }
    }

    public void ResetUIScale()
    {
        SetUIScale(1f);

        if (uiScaleSlider != null)
        {
            uiScaleSlider.SetValueWithoutNotify(1f);
        }
    }

    public void CloseOptionsPanel()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }
}