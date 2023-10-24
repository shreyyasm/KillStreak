using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public GameObject SettingsCanvas;

    public GameObject EngVoiceLinesButton;
    public GameObject HindiVoiceLinesButton;

    public int EngVoiceIndex;
    public int HindiVoiceIndex;

    // Start is called before the first frame update
    void Start()
    {
        LoadSettingsData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangeToEnglish(int EngIndex)
    {
        EngVoiceLinesButton.SetActive(true);
        HindiVoiceLinesButton.SetActive(false);
        PlayerPrefs.SetInt("VoiceSettingsEng", EngIndex);
        PlayerPrefs.SetInt("VoiceSettingsHindi", 0);
        SoundManager.Instance.English = true;
    }
    public void ChangeToHindi(int HindiIndex)
    {
        EngVoiceLinesButton.SetActive(false);
        HindiVoiceLinesButton.SetActive(true);
        PlayerPrefs.SetInt("VoiceSettingsEng", 0);
        PlayerPrefs.SetInt("VoiceSettingsHindi", HindiIndex);
        SoundManager.Instance.English = false;
    }
    public void LoadSettingsData()
    {
        EngVoiceIndex = PlayerPrefs.GetInt("VoiceSettingsEng");
        HindiVoiceIndex = PlayerPrefs.GetInt("VoiceSettingsHindi");
        if(EngVoiceIndex == 1)
        {
            EngVoiceLinesButton.SetActive(true);
            HindiVoiceLinesButton.SetActive(false);
            SoundManager.Instance.English = true;
        }
        else
        {
            EngVoiceLinesButton.SetActive(false);
            HindiVoiceLinesButton.SetActive(true);
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
}
