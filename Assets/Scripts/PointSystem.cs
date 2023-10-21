using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using EOSLobbyTest;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PointSystem : NetworkBehaviour
{
    public static PointSystem Instance;
    [SerializeField] GameManagerEOS gameManagerEOS;

    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    [SerializeField] public int RedTeamScore { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    [SerializeField] public int BlueTeamScore { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

    public TextMeshProUGUI RedScoreText;
    public TextMeshProUGUI BlueScoreText;


    //Game Start Timer
    public TextMeshProUGUI TimerGameStart;
    public float timeRemaining;
    public bool countdownStateStart;

    //Respawn Timer
    public TextMeshProUGUI RespawnTimer;
    public float timeRemainingRespawn;
    public bool countdownStateRespawn;

    

    public PlayerRespawn playerRespawn;
    public bool GameStarted;
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        InvokeRepeating("CheckScore", 20f, 60f);
       // GameStartCountdown();
    }
    
    // Update is called once per frame
    void Update()
    {
        GameStartTimer();
        RespawnStartTimer();
        GameDurationStartTimer();
        CheckWhoWinningNLosing();
    }
    public void CheckScore()
    {
        if (RedTeamScore > BlueTeamScore)
            SoundManager.Instance.PlayRedTeamLeadingLine();

        if (RedTeamScore < BlueTeamScore)
            SoundManager.Instance.PlayBlueTeamLeadingLine();
    }
    public bool red;
    public bool blue;
    bool checkLastScore;
    public void CheckWhoWinningNLosing()
    {
        if(!checkLastScore)
        {
            if (RedTeamScore == 35 || BlueTeamScore == 35)
            {
                if (PlayerManager.Instance.redTeamPlayer)
                {
                    if (RedTeamScore > BlueTeamScore)
                    {
                        SoundManager.Instance.PlayYouAreWinningLine();
                        Debug.Log("Winning");
                    }

                    if (RedTeamScore < BlueTeamScore)
                    {
                        SoundManager.Instance.PlayYouAreLosingLine();
                        
                    }

                }
                if (PlayerManager.Instance.blueTeamPlayer)
                {
                    if (BlueTeamScore > RedTeamScore)
                        SoundManager.Instance.PlayYouAreWinningLine();
                    if (BlueTeamScore < RedTeamScore)
                    {
                        SoundManager.Instance.PlayYouAreLosingLine();
                        Debug.Log("Losing");
                    }
                        
                }
                checkLastScore = true;
            }
        }
        

    }
    public void GameStartCountdown()
    {
        countdownStateStart = true;
        TimerGameStart.enabled = true;
        SoundManager.Instance.PlaySimulationVoiceLine();
    }
    bool countdownSoundPlayed = false;
    bool letsgoSoundPlayed = false;
    public void GameStartTimer()
    {
        if (countdownStateStart)
        {
            //timeRemaining = 5;
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                TimerGameStart.text = "Game Starts In(" + (int)timeRemaining + "s)";
                
                if((int)timeRemaining == 3 && !countdownSoundPlayed)
                {
                    SoundManager.Instance.PlayCountdownLine();
                    countdownSoundPlayed = true;
                }
                if ((int)timeRemaining == 0 && !letsgoSoundPlayed)
                {
                    SoundManager.Instance.PlayLetsGoLine();
                    letsgoSoundPlayed = true;
                }
            }
            else
            {

                TimerGameStart.enabled = false;
                PlayerRespawn.Instance.ResetPosition();
                PlayerRespawn.Instance.HideLoadOutButton();
                GameStarted = true;
                countdownStateStart = false;
                countdownStateGameStop = true;
            }

        }
        else
        {
            timeRemaining = 10;
            //TimerLoadout.text = "TIME'S UP!";

        }
    }
    public void RespawnStartCountdown()
    {
        countdownStateRespawn = true;
        RespawnTimer.enabled = true;
    }
    public void RespawnStartTimer()
    {
        if (countdownStateRespawn)
        {
            //timeRemaining = 5;
            if (timeRemainingRespawn > 0)
            {
                timeRemainingRespawn -= Time.deltaTime;
                RespawnTimer.text = "Respawns In(" + (int)timeRemainingRespawn + "s)";
            }
            else
            {

                RespawnTimer.enabled = false;
                countdownStateRespawn = false;
            }

        }
        else
        {
            timeRemainingRespawn = 10;
            //TimerLoadout.text = "TIME'S UP!";

        }
    }
    //Game Duration Timer
    public TextMeshProUGUI GameDurationTimer;
    public float timeRemainingGameStop;
    public bool countdownStateGameStop;
    bool halfTimePlayed;
    bool gameOverPLayed;
    public void GameDurationStartTimer()
    {
        if (countdownStateGameStop)
        {
            if (timeRemainingGameStop > 0)
            {
                
                timeRemainingGameStop -= Time.deltaTime;
                if ((int)timeRemainingGameStop == 300f && !halfTimePlayed)
                {
                    SoundManager.Instance.PlayHalfTimeLine();
                    halfTimePlayed = true;
                }
                DisplayTime(timeRemainingGameStop);
               
            }
            else
            {
                if(!gameOverPLayed)
                {
                    CancelInvoke("CheckScore");
                    if (RedTeamScore > BlueTeamScore)
                        SoundManager.Instance.PlayRedTeamVictoryLine();
                    if (RedTeamScore < BlueTeamScore)
                        SoundManager.Instance.PlayBlueTeamVictoryLine();
                    gameOverPLayed = true;
                }
                

                    Debug.Log("Time has run out!");
                timeRemainingGameStop = 0;
                countdownStateGameStop = false;
            }
        }
    }
    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        GameDurationTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void AddScoreToRedTeam()
    {
        if (base.IsServer)
            AddScoreToRedTeamObserver();
        else
            AddScoreToRedTeamServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void AddScoreToRedTeamServer()
    {
        RedTeamScore++;
        RedScoreText.text = RedTeamScore.ToString();
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void AddScoreToRedTeamObserver()
    {
        RedTeamScore++;
        RedScoreText.text = RedTeamScore.ToString();
    }
    public void AddScoreToBlueTeam()
    {
        if (base.IsServer)
            AddScoreToBlueTeamObserver();
        else
            AddScoreToBlueTeamServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void AddScoreToBlueTeamServer()
    {
        BlueTeamScore++;
        BlueScoreText.text = BlueTeamScore.ToString();

    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void AddScoreToBlueTeamObserver()
    {
        BlueTeamScore++;
        BlueScoreText.text = BlueTeamScore.ToString();
    }
    public void AddScoreToBoostBlue()
    {
        if (base.IsServer)
            AddScoreToBoostBlueTeamObserver();
        else
            AddScoreToBoostBlueTeamServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void AddScoreToBoostBlueTeamServer()
    {
        BlueTeamScore += 5;
        BlueScoreText.text = BlueTeamScore.ToString();

    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void AddScoreToBoostBlueTeamObserver()
    {
        BlueTeamScore += 5;
        BlueScoreText.text = BlueTeamScore.ToString();
    }
    public void AddScoreToBoostRed()
    {
        if (base.IsServer)
            AddScoreToBoostRedTeamObserver();
        else
            AddScoreToBoostRedTeamServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void AddScoreToBoostRedTeamServer()
    {
        RedTeamScore += 5;
        RedScoreText.text = RedTeamScore.ToString();

    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void AddScoreToBoostRedTeamObserver()
    {
        RedTeamScore += 5;
        RedScoreText.text = RedTeamScore.ToString();
    }
}
