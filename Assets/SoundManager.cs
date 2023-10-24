using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance;

    public bool English;

    [Header("English Voice lines")]
    //_English VoiceLines
    public AudioClip SimulationVoiceClip_English;
    public AudioClip countdownClip_English;
    public AudioClip letsGoClip_English;
    public AudioClip halfTimeClip_English;

    public AudioClip firstKillToRedClip_English;
    public AudioClip firstKillToBlueClip_English;

    public AudioClip redTeamLeadingClip_English;
    public AudioClip blueTeamLeadingClip_English;

    public AudioClip redTeamVictoryClip_English;
    public AudioClip blueTeamVictoryClip_English;

    public AudioClip GreatYouAreWinningClip_English;
    public AudioClip CarefullYouAreLosingClip_English;

    [Header("Hindi Voice lines")]
    
    //_HindiVoiceLines
    public AudioClip SimulationVoiceClip_Hindi;
    public AudioClip countdownClip_Hindi;
    public AudioClip letsGoClip_Hindi;
    public AudioClip halfTimeClip_Hindi;

    public AudioClip firstKillToRedClip_Hindi;
    public AudioClip firstKillToBlueClip_Hindi;

    public AudioClip redTeamLeadingClip_Hindi;
    public AudioClip blueTeamLeadingClip_Hindi;

    public AudioClip redTeamVictoryClip_Hindi;
    public AudioClip blueTeamVictoryClip_Hindi;

    public AudioClip GreatYouAreWinningClip_Hindi;
    public AudioClip CarefullYouAreLosingClip_Hindi;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        
    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        //SimulationVoiceLine();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlaySimulationVoiceLine()
    {
        if (English)
        {
            AudioSource.PlayClipAtPoint(SimulationVoiceClip_English, Camera.main.transform.position, .4f);
            Debug.Log("English Voiceline");
        }

        else
        {
            AudioSource.PlayClipAtPoint(SimulationVoiceClip_Hindi, Camera.main.transform.position, 1f);
            Debug.Log("Hindi Voiceline");
        }
            
    }
    public void PlayCountdownLine()
    {
        if (English)
            AudioSource.PlayClipAtPoint(countdownClip_English, Camera.main.transform.position, 1f);

        else
            AudioSource.PlayClipAtPoint(countdownClip_Hindi, Camera.main.transform.position, 1f);
    }
    public void PlayLetsGoLine()
    {
        if (English)
            AudioSource.PlayClipAtPoint(letsGoClip_English, Camera.main.transform.position, 1f);

        else
            AudioSource.PlayClipAtPoint(letsGoClip_Hindi, Camera.main.transform.position, 1f);
    }
    public void PlayHalfTimeLine()
    {
        if (English)
            AudioSource.PlayClipAtPoint(halfTimeClip_English, Camera.main.transform.position, 1f);

        else
            AudioSource.PlayClipAtPoint(halfTimeClip_Hindi, Camera.main.transform.position, 1f);
    }
    public void PlayRedTeamLeadingLine()
    {
        if (English)
            AudioSource.PlayClipAtPoint(redTeamLeadingClip_English, Camera.main.transform.position, 1f);

        else
            AudioSource.PlayClipAtPoint(redTeamLeadingClip_Hindi, Camera.main.transform.position, 1f);
    }
    public void PlayBlueTeamLeadingLine()
    {
        if (English)
            AudioSource.PlayClipAtPoint(blueTeamLeadingClip_English, Camera.main.transform.position, 1f);

        else
            AudioSource.PlayClipAtPoint(blueTeamLeadingClip_Hindi, Camera.main.transform.position, 1f);
    }
    public void PlayBlueTeamVictoryLine()
    {
        if (English)
            AudioSource.PlayClipAtPoint(blueTeamVictoryClip_English, Camera.main.transform.position, 1f);

        else
            AudioSource.PlayClipAtPoint(blueTeamVictoryClip_Hindi, Camera.main.transform.position, 1f);
    }
    public void PlayRedTeamVictoryLine()
    {
        if (English)
            AudioSource.PlayClipAtPoint(redTeamVictoryClip_English, Camera.main.transform.position, 1f);

        else
            AudioSource.PlayClipAtPoint(redTeamVictoryClip_Hindi, Camera.main.transform.position, 1f);
    }
    public void PlayYouAreWinningLine()
    {
        if (English)
            AudioSource.PlayClipAtPoint(GreatYouAreWinningClip_English, Camera.main.transform.position, 1f);

        else
            AudioSource.PlayClipAtPoint(GreatYouAreWinningClip_Hindi, Camera.main.transform.position, 1f);
    }
    public void PlayYouAreLosingLine()
    {
        if (English)
            AudioSource.PlayClipAtPoint(CarefullYouAreLosingClip_English, Camera.main.transform.position, 1f);

        else
            AudioSource.PlayClipAtPoint(CarefullYouAreLosingClip_Hindi, Camera.main.transform.position, 1f);
    }
    public void PlayfirstKillToRedLine()
    {
        if (English)
            AudioSource.PlayClipAtPoint(firstKillToRedClip_English, Camera.main.transform.position, 1f);

        else
            AudioSource.PlayClipAtPoint(firstKillToRedClip_Hindi, Camera.main.transform.position, 1f);
    }
    public void PlayfirstKillToBlueLine()
    {
        if (English)
            AudioSource.PlayClipAtPoint(firstKillToBlueClip_English, Camera.main.transform.position, 1f);

        else
            AudioSource.PlayClipAtPoint(firstKillToBlueClip_Hindi, Camera.main.transform.position, 1f);
    }
}
