using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Slider volume;
    [SerializeField] private Toggle toggleFullscreen;

    private Resolution[] resolutions;

    private float defaultVolume { get { return PlayerPrefs.HasKey("DefaultVolume") ? PlayerPrefs.GetFloat("DefaultVolume") : 0; } }
    private int defaultQuality { get { return PlayerPrefs.HasKey("DefaultQuality") ? PlayerPrefs.GetInt("DefaultQuality") : 0; } }
    private bool defaultScreen { get { return PlayerPrefs.HasKey("DefaultScreen") ? PlayerPrefs.GetInt("DefaultScreen") != 0 : true; } }
    private int defaultResolution { get { return PlayerPrefs.HasKey("DefaultResolution") ? PlayerPrefs.GetInt("DefaultResolution") : 0; } }

    private void Awake()
    {
        resolutions = Screen.resolutions;
        CreateResolutionList();
    }

    private void Start()
    {
        volume.value = defaultVolume;
        SetVolume(defaultVolume);

        toggleFullscreen.isOn = defaultScreen;
        SetFullscreen(defaultScreen);

        qualityDropdown.value = defaultQuality;
        qualityDropdown.RefreshShownValue();
        SetQuality(defaultQuality);

        resolutionDropdown.value = defaultResolution;
        resolutionDropdown.RefreshShownValue();
        SetResolution(defaultResolution);
    }

    private void CreateResolutionList()
    {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolution = 0;

        for (int current = 0; current < resolutions.Length; current++)
        {
            string option = resolutions[current].width + " x " + resolutions[current].height;
            options.Add(option);

            currentResolution = resolutions[current].width == Screen.currentResolution.width
                && resolutions[current].height == Screen.currentResolution.height ? current : currentResolution;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolution;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetVolume(float volume)
    {
        mixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat("DefaultVolume", volume);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("DefaultQuality", index);
    }

    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("DefaultScreen", fullscreen ? 1 : 0);
    }

    public void SetResolution(int index)
    {
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("DefaultResolution", index);
    }
}
