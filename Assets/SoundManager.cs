using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance;

    //Public Lines
    public AudioClip SimulationVoiceClip;
    public AudioClip countdownClip;
    public AudioClip letsGoClip;
    public AudioClip halfTimeClip;

    public AudioClip firstKillToRedClip;
    public AudioClip firstKillToBlueClip;

    public AudioClip redTeamLeadingClip;
    public AudioClip blueTeamLeadingClip;

    public AudioClip redTeamVictoryClip;
    public AudioClip blueTeamVictoryClip;

    public AudioClip GreatYouAreWinningClip;
    public AudioClip CarefullYouAreLosingClip;


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
        AudioSource.PlayClipAtPoint(SimulationVoiceClip, Camera.main.transform.position, 1f);
    }
    public void PlayCountdownLine()
    {
        AudioSource.PlayClipAtPoint(countdownClip, Camera.main.transform.position, 1f);
    }
    public void PlayLetsGoLine()
    {
        AudioSource.PlayClipAtPoint(letsGoClip, Camera.main.transform.position, 1f);
    }
    public void PlayHalfTimeLine()
    {
        AudioSource.PlayClipAtPoint(halfTimeClip, Camera.main.transform.position, 1f);
    }
    public void PlayRedTeamLeadingLine()
    {
        AudioSource.PlayClipAtPoint(redTeamLeadingClip, Camera.main.transform.position, 1f);
    }
    public void PlayBlueTeamLeadingLine()
    {
        AudioSource.PlayClipAtPoint(blueTeamLeadingClip, Camera.main.transform.position, 1f);
    }
    public void PlayBlueTeamVictoryLine()
    {
        AudioSource.PlayClipAtPoint(blueTeamVictoryClip, Camera.main.transform.position, 1f);
    }
    public void PlayRedTeamVictoryLine()
    {
        AudioSource.PlayClipAtPoint(redTeamVictoryClip, Camera.main.transform.position, 1f);
    }
    public void PlayYouAreWinningLine()
    {
        AudioSource.PlayClipAtPoint(GreatYouAreWinningClip, Camera.main.transform.position, 1f);
    }
    public void PlayYouAreLosingLine()
    {
        AudioSource.PlayClipAtPoint(CarefullYouAreLosingClip, Camera.main.transform.position, 1f);
    }
    public void PlayfirstKillToRedLine()
    {
        AudioSource.PlayClipAtPoint(firstKillToRedClip, Camera.main.transform.position, 1f);
    }
    public void PlayfirstKillToBlueLine()
    {
        AudioSource.PlayClipAtPoint(firstKillToBlueClip, Camera.main.transform.position, 1f);
    }
}
