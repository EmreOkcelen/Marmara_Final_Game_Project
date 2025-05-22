using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Graphics")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public TMP_Dropdown qualityDropdown;

    [Header("Controls")]
    public Slider mouseSensitivitySlider;
    public Toggle invertYAxisToggle;

    [Header("Buttons")]
    public Button applyButton;
    public Button cancelButton;

    private Resolution[] resolutions;
    private int currentResolutionIndex;

    private void Start()
    {
        resolutions = Screen.resolutions;
        SetupResolutionDropdown();

        LoadSettings();

        applyButton.onClick.AddListener(ApplySettings);
        cancelButton.onClick.AddListener(LoadSettings);
    }

    private void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void ApplySettings()
    {
        // Ses
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);

        // Grafik
        int resIndex = resolutionDropdown.value;
        Resolution res = resolutions[resIndex];
        Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn);
        PlayerPrefs.SetInt("ResolutionIndex", resIndex);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("QualityLevel", qualityDropdown.value);
        QualitySettings.SetQualityLevel(qualityDropdown.value);

        // Kontroller
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);
        PlayerPrefs.SetInt("InvertY", invertYAxisToggle.isOn ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        // Ses
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        // Grafik
        int resIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        resolutionDropdown.value = resIndex;
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", 2);

        // Kontrol
        mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        invertYAxisToggle.isOn = PlayerPrefs.GetInt("InvertY", 0) == 1;

        // Uygula (anlýk)
        ApplySettings(); // Ekrana uygula ama kaydetmeden
    }
}
