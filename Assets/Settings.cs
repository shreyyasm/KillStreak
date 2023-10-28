using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public static Settings Instance;
    public GameObject SettingsCanvas;

    public GameObject EngVoiceLinesButton;
    public GameObject HindiVoiceLinesButton;

    public int EngVoiceIndex;
    public int HindiVoiceIndex;

    public float MusicVolume;
    public Slider VolumeSlider;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        LoadSettingsData();
    }

    // Update is called once per frame
    void Update()
    {
        SetMusicVolume();
    }
    public void ChangeToEnglish(int EngIndex)
    {
        EngVoiceLinesButton.SetActive(true);
        HindiVoiceLinesButton.SetActive(false);
        PlayerPrefs.SetInt("VoiceSettingsEng", EngIndex);
        PlayerPrefs.SetInt("VoiceSettingsHindi", 0);
        if (SoundManager.Instance != null)
            SoundManager.Instance.English = true;
    }
    public void ChangeToHindi(int HindiIndex)
    {
        EngVoiceLinesButton.SetActive(false);
        HindiVoiceLinesButton.SetActive(true);
        PlayerPrefs.SetInt("VoiceSettingsEng", 0);
        PlayerPrefs.SetInt("VoiceSettingsHindi", HindiIndex);
        if (SoundManager.Instance != null)
            SoundManager.Instance.English = false;
    }
    public void LoadSettingsData()
    {
        EngVoiceIndex = PlayerPrefs.GetInt("VoiceSettingsEng");
        HindiVoiceIndex = PlayerPrefs.GetInt("VoiceSettingsHindi");
        VolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        if (EngVoiceIndex == 1)
        {
            EngVoiceLinesButton.SetActive(true);
            HindiVoiceLinesButton.SetActive(false);
            if (SoundManager.Instance != null)
                SoundManager.Instance.English = true;
        }
        else
        {
            EngVoiceLinesButton.SetActive(false);
            HindiVoiceLinesButton.SetActive(true);
            if(SoundManager.Instance != null)
                SoundManager.Instance.English = false;
        }


    }
    public void OpenInGameSettings()
    {
        SettingsCanvas.SetActive(true);
    }
    public void CloseSettings()
    {
        SettingsCanvas.SetActive(false);
    }
    public void SetMusicVolume()
    {
        MusicVolume = VolumeSlider.value;
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
    }
}
